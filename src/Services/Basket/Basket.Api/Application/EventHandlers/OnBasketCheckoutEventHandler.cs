using Basket.Api.Domain.Events;
using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.Interfaces;

namespace Basket.Api.Application.EventHandlers;

public class OnBasketCheckoutEventHandler(IAppLogger<OnBasketCheckoutEventHandler> logger, IEventBus eventBus)
    : IDomainEventHandler<BasketCheckoutDomainEvent>
{
    public async Task HandleAsync(BasketCheckoutDomainEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Application, "Basket checkout domain event received.");

        BasketCheckoutIntegrationEvent integrationEvent = new() { };
        await eventBus.PublishAsync(integrationEvent, cancellationToken);

        logger.LogInformation(LogTypeEnum.Application, "Basket checkout integration event published.");
    }
}
