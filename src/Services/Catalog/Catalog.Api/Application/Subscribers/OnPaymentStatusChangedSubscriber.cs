using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;
using BuildingBlocks.Messaging.Interfaces;
using Catalog.Api.Application.Catalog.UpdateStock;

namespace Catalog.Api.Application.Subscribers;

/// <summary>
/// Restores stock for every catalog item in the order when payment fails, is rejected or cancelled.
/// Publishes CatalogStockUpdatedIntegrationEvent so the merchant dashboard can update in real-time.
/// </summary>
public class OnPaymentStatusChangedSubscriber(
    IAppLogger<OnPaymentStatusChangedSubscriber> logger,
    UpdateStock updateStock,
    IEventBus eventBus)
    : IIntegrationEventHandler<PaymentStatusChangedIntegrationEvent>
{
    // PaymentStatusEnum: 5=Failed, 6=Rejected, 7=Cancelled
    private static readonly HashSet<int> _stockReleaseStatuses = [5, 6, 7];

    public async Task HandleAsync(PaymentStatusChangedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        if (!_stockReleaseStatuses.Contains(@event.PaymentStatus))
            return;

        logger.LogInformation(LogTypeEnum.Application,
            "Payment {Status} event received in Catalog Service. OrderId: {OrderId}, Releasing stock for {ItemCount} item(s), CorrelationId: {CorrelationId}",
            @event.PaymentStatusName, @event.OrderId, @event.Items.Count, @event.CorrelationId);

        foreach (OrderItemData item in @event.Items)
        {
            BuildingBlocks.Result<UpdateStockResult> result =
                await updateStock.HandleAsync(new UpdateStockCommand(item.CatalogId, +item.Quantity));

            if (result.IsSuccess)
            {
                logger.LogInformation(LogTypeEnum.Business,
                    "Stock restored for catalog {CatalogId} by {Quantity}. New stock: {NewStock}",
                    item.CatalogId, item.Quantity, result.Data!.NewStock);

                await eventBus.PublishAsync(new CatalogStockUpdatedIntegrationEvent
                {
                    CatalogId = item.CatalogId,
                    MerchantId = result.Data.MerchantId,
                    ProductName = result.Data.ProductName,
                    NewStock = result.Data.NewStock,
                    CorrelationId = @event.CorrelationId
                }, cancellationToken);
            }
            else
            {
                logger.LogError(LogTypeEnum.Business, null,
                    "Failed to restore stock for catalog {CatalogId}: {Error}",
                    item.CatalogId, result.Error);
            }
        }
    }
}
