using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.Interfaces;
using Order.Api.Application.Subscribers;

namespace Order.Api.Application.HostedService;

/// <summary>
/// Hosted service that subscribes to integration events on application startup
/// </summary>
public class IntegrationEventSubscriptionService(IEventBus eventBus) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Subscribe to integration events
        eventBus.Subscribe<BasketCheckoutIntegrationEvent, OnBasketCheckoutSubscriber>();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}