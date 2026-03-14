using BackendForFrontend.Api.Services;
using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

namespace BackendForFrontend.Api.Catalog.Subscribers;

/// <summary>
/// Receives StockReservationFailedIntegrationEvent and pushes a real-time alert
/// to the merchant's SSE stream so the dashboard can display the failure.
/// </summary>
public class OnStockReservationFailedSubscriber(
    IAppLogger<OnStockReservationFailedSubscriber> logger,
    IMerchantAlertService merchantAlertService,
    IMerchantUserMappingService merchantUserMappingService)
    : IIntegrationEventHandler<StockReservationFailedIntegrationEvent>
{
    public async Task HandleAsync(StockReservationFailedIntegrationEvent @event,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Application,
            "Stock reservation failed for order {OrderId}, merchant {MerchantId}. Pushing alert. CorrelationId: {CorrelationId}",
            @event.OrderId, @event.MerchantId, @event.CorrelationId);

        string merchantId = @event.MerchantId.ToString();

        if (!merchantUserMappingService.TryGetUserId(merchantId, out string? userId) || userId is null)
        {
            logger.LogInformation(LogTypeEnum.Application,
                "No active SSE connection for merchant {MerchantId}. Alert not pushed (merchant offline).",
                merchantId);
            return;
        }

        await merchantAlertService.PublishAsync(userId, new MerchantAlertEvent(
            OrderId: @event.OrderId,
            CatalogId: @event.CatalogId,
            ProductName: @event.ProductName,
            RequestedQuantity: @event.RequestedQuantity,
            AvailableQuantity: @event.AvailableQuantity,
            Reason: @event.FailureReason,
            OccurredAt: DateTimeOffset.UtcNow));

        logger.LogInformation(LogTypeEnum.Application,
            "Merchant alert pushed for order {OrderId} to merchant {MerchantId}.",
            @event.OrderId, merchantId);
    }
}
