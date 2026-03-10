using BackendForFrontend.Api.Services;
using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

namespace BackendForFrontend.Api.Catalog.Subscribers;

/// <summary>
/// Receives CatalogStockUpdatedIntegrationEvent from RabbitMQ and pushes the change
/// to the merchant's connected SSE stream. Uses IMerchantUserMappingService to map
/// merchantId (from the catalog entity) to the userId (SSE channel key) for merchants
/// who are currently viewing the dashboard.
/// </summary>
public class OnCatalogStockUpdatedSubscriber(
    IAppLogger<OnCatalogStockUpdatedSubscriber> logger,
    IStockEventService stockEventService,
    IMerchantUserMappingService merchantUserMappingService)
    : IIntegrationEventHandler<CatalogStockUpdatedIntegrationEvent>
{
    public async Task HandleAsync(CatalogStockUpdatedIntegrationEvent @event,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Application,
            "Catalog stock updated event received. CatalogId: {CatalogId}, MerchantId: {MerchantId}, NewStock: {NewStock}",
            @event.CatalogId, @event.MerchantId, @event.NewStock);

        string merchantId = @event.MerchantId.ToString();

        if (!merchantUserMappingService.TryGetUserId(merchantId, out string? userId) || userId is null)
        {
            logger.LogInformation(LogTypeEnum.Application,
                "No active SSE connection for merchant {MerchantId}. Skipping SSE push.",
                merchantId);
            return;
        }

        await stockEventService.PublishAsync(userId,
            new StockChangedEvent(@event.CatalogId, @event.ProductName, @event.NewStock, DateTimeOffset.UtcNow));

        logger.LogInformation(LogTypeEnum.Application,
            "SSE stock update pushed for catalog {CatalogId} to merchant {MerchantId}.",
            @event.CatalogId, merchantId);
    }
}
