using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;
using Payment.Api.Application.Payment.PatchPaymentStatus;
using Payment.Api.Domain.Entities;
using Payment.Api.Domain.Enums;
using Payment.Api.Domain.Repositories;

namespace Payment.Api.Application.Subscribers;

/// <summary>
/// Cancels the payment when stock reservation fails.
/// Retries finding the payment to handle the race condition where the
/// PaymentCreated handler may not have run yet when this event arrives.
/// </summary>
public class OnStockReservationFailedSubscriber(
    IAppLogger<OnStockReservationFailedSubscriber> logger,
    IPaymentRepository paymentRepository,
    PatchPaymentStatus patchPaymentStatus)
    : IIntegrationEventHandler<StockReservationFailedIntegrationEvent>
{
    private const int MaxRetries = 5;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

    public async Task HandleAsync(StockReservationFailedIntegrationEvent @event,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Application,
            "Stock reservation failed for order {OrderId} — cancelling payment. CorrelationId: {CorrelationId}",
            @event.OrderId, @event.CorrelationId);

        PaymentEntity? payment = null;

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            payment = await paymentRepository.GetByOrderIdAsync(@event.OrderId, isTrackingEnabled: true, cancellationToken);

            if (payment is not null)
                break;

            if (attempt < MaxRetries)
            {
                logger.LogWarning(LogTypeEnum.Application,
                    "Payment for order {OrderId} not found yet (attempt {Attempt}/{MaxRetries}). Retrying in {Delay}s...",
                    @event.OrderId, attempt, MaxRetries, RetryDelay.TotalSeconds);

                await Task.Delay(RetryDelay, cancellationToken);
            }
        }

        if (payment is null)
        {
            logger.LogError(LogTypeEnum.Business, null,
                "Payment for order {OrderId} not found after {MaxRetries} attempts. Cannot cancel. CorrelationId: {CorrelationId}",
                @event.OrderId, MaxRetries, @event.CorrelationId);
            return;
        }

        // Only cancel if payment hasn't already reached a terminal state
        if (payment.Status is PaymentStatusEnum.Cancelled or PaymentStatusEnum.Refunded
            or PaymentStatusEnum.Failed or PaymentStatusEnum.Rejected)
        {
            logger.LogInformation(LogTypeEnum.Application,
                "Payment {PaymentId} for order {OrderId} is already in terminal status {Status}. Skipping.",
                payment.Id, @event.OrderId, payment.Status);
            return;
        }

        BuildingBlocks.Result<PatchPaymentStatusResult> result = await patchPaymentStatus.HandleAsync(
            new PatchPaymentStatusCommand { Id = payment.Id.Value, Status = PaymentStatusEnum.Cancelled });

        if (result.IsSuccess)
        {
            logger.LogInformation(LogTypeEnum.Business,
                "Payment {PaymentId} cancelled for order {OrderId} due to stock reservation failure. CorrelationId: {CorrelationId}",
                payment.Id, @event.OrderId, @event.CorrelationId);
        }
        else
        {
            logger.LogError(LogTypeEnum.Business, null,
                "Failed to cancel payment {PaymentId} for order {OrderId}: {Error}. CorrelationId: {CorrelationId}",
                payment.Id, @event.OrderId, result.Error, @event.CorrelationId);
        }
    }
}
