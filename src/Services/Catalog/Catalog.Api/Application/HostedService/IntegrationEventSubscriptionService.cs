using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.Interfaces;
using Catalog.Api.Application.Subscribers;

namespace Catalog.Api.Application.HostedService;

/// <summary>
/// Hosted service that subscribes to integration events on application startup.
/// </summary>
public class IntegrationEventSubscriptionService(IEventBus eventBus) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        eventBus.Subscribe<OrderCreatedIntegrationEvent, OnOrderCreatedSubscriber>();
        eventBus.Subscribe<PaymentStatusChangedIntegrationEvent, OnPaymentStatusChangedSubscriber>();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
