using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;

namespace Catalog.Api.Application.Subscribers;

/// <summary>
/// Handles payment status changes:
/// - Authorized/Captured  → confirms reservation (removes from Reserved)
/// - Failed/Rejected/Cancelled/Refunded/Chargeback → releases reservation back to Available
///
/// Stock mutations call <see cref="CatalogEntity.ConfirmReservation"/> or
/// <see cref="CatalogEntity.ReleaseReservation"/>, which raise a domain event that is
/// saved atomically to the Outbox and published by the OutboxProcessor.
/// </summary>
public class OnPaymentStatusChangedSubscriber(
    IAppLogger<OnPaymentStatusChangedSubscriber> logger,
    ICatalogRepository repository)
    : IIntegrationEventHandler<PaymentStatusChangedIntegrationEvent>
{
    private static readonly HashSet<int> ConfirmStatuses =
        [PaymentStatusCodes.Authorized, PaymentStatusCodes.Captured];

    private static readonly HashSet<int> ReleaseStatuses =
    [
        PaymentStatusCodes.Failed, PaymentStatusCodes.Rejected, PaymentStatusCodes.Cancelled,
        PaymentStatusCodes.Refunded, PaymentStatusCodes.Chargeback
    ];

    public async Task HandleAsync(PaymentStatusChangedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        bool isConfirm = ConfirmStatuses.Contains(@event.PaymentStatus);
        bool isRelease = ReleaseStatuses.Contains(@event.PaymentStatus);

        if (!isConfirm && !isRelease)
            return;

        string operation = isConfirm ? "Confirming" : "Releasing";
        logger.LogInformation(LogTypeEnum.Application,
            "Payment {Status} — {Operation} reservation for order {OrderId}, {ItemCount} item(s). CorrelationId: {CorrelationId}",
            @event.PaymentStatusName, operation, @event.OrderId, @event.Items.Count, @event.CorrelationId);

        foreach (OrderItemData item in @event.Items)
        {
            CatalogEntity? entity = await repository.GetByIdAsync(
                CatalogId.Of(item.CatalogId), isTrackingEnabled: true);

            if (entity is null)
            {
                logger.LogError(LogTypeEnum.Business, null,
                    "Catalog {CatalogId} not found while processing payment status change.", item.CatalogId);
                continue;
            }

            try
            {
                if (isConfirm)
                    entity.ConfirmReservation(item.Quantity, @event.CorrelationId);
                else
                    entity.ReleaseReservation(item.Quantity, @event.CorrelationId);

                await repository.UpdateAsync(entity);

                logger.LogInformation(LogTypeEnum.Business,
                    "Reservation {Operation} for catalog {CatalogId} — {Quantity} unit(s). Available: {Available}, Reserved: {Reserved}",
                    operation.ToLowerInvariant(), item.CatalogId, item.Quantity,
                    entity.Stock.Available, entity.Stock.Reserved);
            }
            catch (Exception ex)
            {
                logger.LogError(LogTypeEnum.Exception, ex,
                    "Error processing reservation {Operation} for catalog {CatalogId}.",
                    operation.ToLowerInvariant(), item.CatalogId);
            }
        }
    }
}
