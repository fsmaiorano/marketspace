using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.Interfaces;
using Order.Api.Domain.Events;

namespace Order.Api.Application.EventHandlers;

public class OnOrderCreatedEventHandler(
    IAppLogger<OnOrderCreatedEventHandler> logger,
    IEventBus eventBus) : IDomainEventHandler<OrderCreatedDomainEvent>
{
    public async Task HandleAsync(OrderCreatedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Application,
            "Order created domain event received. OrderId: {OrderId}, CustomerId: {CustomerId}, CorrelationId: {CorrelationId}",
            @event.OrderId, @event.CustomerId, @event.CorrelationId);

        OrderCreatedIntegrationEvent integrationEvent = new()
        {
            CorrelationId = @event.CorrelationId,
            OrderId = @event.OrderId,
            CustomerId = @event.CustomerId,
            TotalAmount = @event.TotalAmount,
            CardNumber = @event.CardNumber,
            CardName = @event.CardName,
            Expiration = @event.Expiration,
            Cvv = @event.Cvv,
            PaymentMethod = @event.PaymentMethod,
            Items = @event.Items
        };

        await eventBus.PublishAsync(integrationEvent, cancellationToken);

        logger.LogInformation(LogTypeEnum.Application,
            "Order created integration event published. OrderId: {OrderId}, CustomerId: {CustomerId}, TotalAmount: {TotalAmount}, CorrelationId: {CorrelationId}",
            @event.OrderId, @event.CustomerId, @event.TotalAmount, @event.CorrelationId);
    }
}