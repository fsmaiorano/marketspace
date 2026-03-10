using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;
using BuildingBlocks.Messaging.Interfaces;
using Catalog.Api.Application.Catalog.UpdateStock;

namespace Catalog.Api.Application.Subscribers;

/// <summary>
/// Decrements stock for every catalog item in the order when an order is created.
/// Publishes CatalogStockUpdatedIntegrationEvent so the merchant dashboard can update in real-time.
/// </summary>
public class OnOrderCreatedSubscriber(
    IAppLogger<OnOrderCreatedSubscriber> logger,
    UpdateStock updateStock,
    IEventBus eventBus)
    : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    public async Task HandleAsync(OrderCreatedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Application,
            "Order created event received in Catalog Service. OrderId: {OrderId}, ItemCount: {ItemCount}, CorrelationId: {CorrelationId}",
            @event.OrderId, @event.Items.Count, @event.CorrelationId);

        foreach (OrderItemData item in @event.Items)
        {
            BuildingBlocks.Result<UpdateStockResult> result =
                await updateStock.HandleAsync(new UpdateStockCommand(item.CatalogId, -item.Quantity));

            if (result.IsSuccess)
            {
                logger.LogInformation(LogTypeEnum.Business,
                    "Stock decremented for catalog {CatalogId} by {Quantity}. New stock: {NewStock}",
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
                    "Failed to decrement stock for catalog {CatalogId}: {Error}",
                    item.CatalogId, result.Error);
            }
        }
    }
}
