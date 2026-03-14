using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;
using Catalog.Api.Application.Catalog.ReserveStock;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;

namespace Catalog.Api.Application.Subscribers;

/// <summary>
/// Reserves stock for every item in the order when an order is created.
/// If any reservation fails, already-reserved items are compensated (released)
/// and a <see cref="Catalog.Api.Domain.Events.CatalogStockReservationFailedDomainEvent"/>
/// is raised on the failed entity — the Outbox then delivers
/// <see cref="StockReservationFailedIntegrationEvent"/> reliably to Order, Payment, and BFF.
/// </summary>
public class OnOrderCreatedSubscriber(
    IAppLogger<OnOrderCreatedSubscriber> logger,
    ReserveStock reserveStock,
    ICatalogRepository catalogRepository)
    : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    public async Task HandleAsync(OrderCreatedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Application,
            "Order {OrderId} created — reserving stock for {ItemCount} item(s). CorrelationId: {CorrelationId}",
            @event.OrderId, @event.Items.Count, @event.CorrelationId);

        List<OrderItemData> reserved = [];

        foreach (OrderItemData item in @event.Items)
        {
            BuildingBlocks.Result<ReserveStockResult> result =
                await reserveStock.HandleAsync(
                    new ReserveStockCommand(item.CatalogId, item.Quantity, @event.CorrelationId),
                    cancellationToken);

            if (!result.IsSuccess)
            {
                logger.LogError(LogTypeEnum.Business, null,
                    "Failed to reserve stock for catalog {CatalogId}: {Error}. Compensating {Count} previously reserved item(s).",
                    item.CatalogId, result.Error, reserved.Count);

                await CompensateReservedItemsAsync(reserved, @event.CorrelationId, cancellationToken);

                CatalogEntity? failedEntity = await catalogRepository.GetByIdAsync(
                    CatalogId.Of(item.CatalogId), isTrackingEnabled: true);

                if (failedEntity is not null)
                {
                    failedEntity.RaiseReservationFailed(
                        orderId: @event.OrderId,
                        customerId: @event.CustomerId,
                        requestedQuantity: item.Quantity,
                        availableQuantity: failedEntity.Stock.Available,
                        failureReason: result.Error ?? "Insufficient stock.",
                        items: @event.Items,
                        correlationId: @event.CorrelationId);

                    await catalogRepository.UpdateAsync(failedEntity);
                }
                else
                {
                    logger.LogError(LogTypeEnum.Exception, null,
                        "Catalog {CatalogId} not found — StockReservationFailed domain event could not be raised. OrderId: {OrderId}",
                        item.CatalogId, @event.OrderId);
                }

                return;
            }

            reserved.Add(item);

            logger.LogInformation(LogTypeEnum.Business,
                "Stock reserved for catalog {CatalogId} — {Quantity} unit(s). Available: {Available}, Reserved: {Reserved}",
                item.CatalogId, item.Quantity, result.Data!.Available, result.Data.Reserved);
        }
    }

    /// <summary>
    /// Releases reservations already made before a failure — compensating transaction.
    /// </summary>
    private async Task CompensateReservedItemsAsync(
        List<OrderItemData> reserved,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        foreach (OrderItemData item in reserved)
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

                entity.ReleaseReservation(item.Quantity, correlationId);
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
