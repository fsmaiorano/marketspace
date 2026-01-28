using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.IntegrationEvents;
using Payment.Api.Application.Subscribers;

namespace Payment.Api.Application.HostedService;

/// <summary>
/// Hosted service that subscribes to integration events on application startup
/// </summary>
public class IntegrationEventSubscriptionService(IEventBus eventBus) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Subscribe to integration events
        eventBus.Subscribe<OrderCreatedIntegrationEvent, OnOrderCreatedSubscriber>();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}