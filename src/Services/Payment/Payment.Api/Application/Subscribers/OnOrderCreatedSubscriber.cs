using BuildingBlocks;
using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;
using Payment.Api.Application.Payment.CreatePayment;
using Payment.Api.Domain.ValueObjects;

namespace Payment.Api.Application.Subscribers;

public class OnOrderCreatedSubscriber(
    IAppLogger<OnOrderCreatedSubscriber> logger,
    ICreatePaymentHandler createPaymentHandler)
    : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    public async Task HandleAsync(OrderCreatedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Application,
            "Order created integration event received in Payment Service. OrderId: {OrderId}, CustomerId: {CustomerId}, TotalAmount: {TotalAmount}, CorrelationId: {CorrelationId}",
            @event.OrderId, @event.CustomerId, @event.TotalAmount, @event.CorrelationId);

        CreatePaymentCommand command = new CreatePaymentCommand
        {
            OrderId = @event.OrderId,
            Amount = @event.TotalAmount,
            Currency = "BRL",
            Provider = @event.CardName,
            Method = @event.PaymentMethod
        };

        Result<CreatePaymentResult> result = await createPaymentHandler.HandleAsync(command);

        if (result.IsSuccess)
        {
            logger.LogInformation(LogTypeEnum.Business,
                $"Payment created successfully. PaymentId: {result.Data?.PaymentId}",
                result.Data?.PaymentId, command.OrderId);
        }
        else
        {
            logger.LogError(LogTypeEnum.Business, null,
                "Failed to create payment for order: {CustomerId}. Error: {Error}",
                command.OrderId, result.Error);
        }
    }
}