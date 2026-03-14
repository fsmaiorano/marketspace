using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;
using BuildingBlocks.Messaging.Interfaces;
using Catalog.Api.Application.Catalog.ReserveStock;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;

namespace Catalog.Api.Application.Subscribers;

/// <summary>
/// Reserves stock for every item in the order when an order is created.
/// If any reservation fails, already-reserved items are compensated (released)
/// and a StockReservationFailedIntegrationEvent is published to trigger
/// order and payment cancellation and merchant notification.
/// </summary>
public class OnOrderCreatedSubscriber(
    IAppLogger<OnOrderCreatedSubscriber> logger,
    ReserveStock reserveStock,
    ICatalogRepository catalogRepository,
    IEventBus eventBus)
    : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    public async Task HandleAsync(OrderCreatedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Application,
            "Order {OrderId} created — reserving stock for {ItemCount} item(s). CorrelationId: {CorrelationId}",
            @event.OrderId, @event.Items.Count, @event.CorrelationId);

        List<(OrderItemData Item, ReserveStockResult StockResult)> reserved = [];

        foreach (OrderItemData item in @event.Items)
        {
            BuildingBlocks.Result<ReserveStockResult> result =
                await reserveStock.HandleAsync(new ReserveStockCommand(item.CatalogId, item.Quantity), cancellationToken);

            if (!result.IsSuccess)
            {
                logger.LogError(LogTypeEnum.Business, null,
                    "Failed to reserve stock for catalog {CatalogId}: {Error}. Compensating {Count} previously reserved item(s).",
                    item.CatalogId, result.Error, reserved.Count);

                await CompensateReservedItemsAsync(reserved, cancellationToken);

                // Look up product info for the notification
                CatalogEntity? failedEntity = await catalogRepository.GetByIdAsync(
                    CatalogId.Of(item.CatalogId), isTrackingEnabled: false);

                await eventBus.PublishAsync(new StockReservationFailedIntegrationEvent
                {
                    OrderId = @event.OrderId,
                    CustomerId = @event.CustomerId,
                    MerchantId = failedEntity?.MerchantId ?? Guid.Empty,
                    ProductName = failedEntity?.Name ?? item.CatalogId.ToString(),
                    CatalogId = item.CatalogId,
                    RequestedQuantity = item.Quantity,
                    AvailableQuantity = failedEntity?.Stock.Available ?? 0,
                    FailureReason = result.Error ?? "Insufficient stock.",
                    Items = @event.Items,
                    CorrelationId = @event.CorrelationId
                }, cancellationToken);

                return;
            }

            reserved.Add((item, result.Data!));

            logger.LogInformation(LogTypeEnum.Business,
                "Stock reserved for catalog {CatalogId} — {Quantity} unit(s). Available: {Available}, Reserved: {Reserved}",
                item.CatalogId, item.Quantity, result.Data!.Available, result.Data.Reserved);

            await eventBus.PublishAsync(new CatalogStockUpdatedIntegrationEvent
            {
                CatalogId = item.CatalogId,
                MerchantId = result.Data.MerchantId,
                ProductName = result.Data.ProductName,
                Available = result.Data.Available,
                Reserved = result.Data.Reserved,
                CorrelationId = @event.CorrelationId
            }, cancellationToken);
        }
    }

    /// <summary>
    /// Releases reservations already made before a failure — compensating transaction.
    /// </summary>
    private async Task CompensateReservedItemsAsync(
        List<(OrderItemData Item, ReserveStockResult StockResult)> reserved,
        CancellationToken cancellationToken)
    {
        foreach ((OrderItemData item, _) in reserved)
        {
            try
            {
                CatalogEntity? entity = await catalogRepository.GetByIdAsync(
                    CatalogId.Of(item.CatalogId), isTrackingEnabled: true);

                if (entity is null)
                {
                    logger.LogWarning(LogTypeEnum.Application,
                        "Catalog {CatalogId} not found during compensation.", item.CatalogId);
                    continue;
                }

                entity.ReleaseReservation(item.Quantity);
                await catalogRepository.UpdateAsync(entity);

                logger.LogInformation(LogTypeEnum.Business,
                    "Compensation: released reservation for catalog {CatalogId}, qty {Quantity}.",
                    item.CatalogId, item.Quantity);
            }
            catch (Exception ex)
            {
                logger.LogError(LogTypeEnum.Exception, ex,
                    "Failed to compensate reservation for catalog {CatalogId}.", item.CatalogId);
            }
        }
    }
}
