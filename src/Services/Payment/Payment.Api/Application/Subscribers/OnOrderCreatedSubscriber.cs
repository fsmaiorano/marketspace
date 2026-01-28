using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

namespace Payment.Api.Application.Subscribers;

public class OnOrderCreatedSubscriber(IAppLogger<OnOrderCreatedSubscriber> logger)
    : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    public Task HandleAsync(OrderCreatedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}