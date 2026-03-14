using BackendForFrontend.Api.Catalog.Subscribers;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.Interfaces;

namespace BackendForFrontend.Api.HostedService;

/// <summary>
/// Hosted service that subscribes to integration events on application startup.
/// </summary>
public class IntegrationEventSubscriptionService(IEventBus eventBus) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        eventBus.Subscribe<CatalogStockUpdatedIntegrationEvent, OnCatalogStockUpdatedSubscriber>();
        eventBus.Subscribe<StockReservationFailedIntegrationEvent, OnStockReservationFailedSubscriber>();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
