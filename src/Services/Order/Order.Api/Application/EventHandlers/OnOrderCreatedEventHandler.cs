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
            @event.Order.Id.Value, @event.Order.CustomerId.Value, @event.CorrelationId);

        OrderCreatedIntegrationEvent integrationEvent = new()
        {
            CorrelationId = @event.CorrelationId,
            OrderId = @event.Order.Id.Value,
            CustomerId = @event.Order.CustomerId.Value,
            TotalAmount = @event.Order.TotalAmount.Value
        };
        
        await eventBus.PublishAsync(integrationEvent, cancellationToken);

        logger.LogInformation(LogTypeEnum.Application, 
            "Order created integration event published. OrderId: {OrderId}, CustomerId: {CustomerId}, TotalAmount: {TotalAmount}, CorrelationId: {CorrelationId}",
            @event.Order.Id.Value, @event.Order.CustomerId.Value, @event.Order.TotalAmount.Value, @event.CorrelationId);
    }
}