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
        logger.LogInformation(LogTypeEnum.Application, "Order created domain event received.");

        OrderCreatedIntegrationEvent integrationEvent = new() { };
        await eventBus.PublishAsync(integrationEvent, cancellationToken);

        logger.LogInformation(LogTypeEnum.Application, "Order created integration event published.");
    }
}