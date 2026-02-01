using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

namespace Order.Api.Application.Subscribers;

public class OnBasketCheckoutSubscriber(IAppLogger<OnBasketCheckoutSubscriber> logger)
    : IIntegrationEventHandler<BasketCheckoutIntegrationEvent>
{
    public Task HandleAsync(BasketCheckoutIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        
        //CreateOrder!
        throw new NotImplementedException();
    }
}