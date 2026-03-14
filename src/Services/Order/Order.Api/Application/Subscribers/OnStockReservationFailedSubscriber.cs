using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;
using Order.Api.Application.Order.PatchOrderStatus;
using Order.Api.Domain.Enums;

namespace Order.Api.Application.Subscribers;

/// <summary>
/// Cancels the order when stock reservation fails in the Catalog service.
/// </summary>
public class OnStockReservationFailedSubscriber(
    IAppLogger<OnStockReservationFailedSubscriber> logger,
    PatchOrderStatus patchOrderStatus)
    : IIntegrationEventHandler<StockReservationFailedIntegrationEvent>
{
    public async Task HandleAsync(StockReservationFailedIntegrationEvent @event,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Application,
            "Stock reservation failed for order {OrderId} — cancelling order. Reason: {Reason}. CorrelationId: {CorrelationId}",
            @event.OrderId, @event.FailureReason, @event.CorrelationId);

        BuildingBlocks.Result<PatchOrderStatusResult> result = await patchOrderStatus.HandleAsync(
            new PatchOrderStatusCommand { Id = @event.OrderId, Status = OrderStatusEnum.Cancelled });

        if (result.IsSuccess)
        {
            logger.LogInformation(LogTypeEnum.Business,
                "Order {OrderId} cancelled due to stock reservation failure. CorrelationId: {CorrelationId}",
                @event.OrderId, @event.CorrelationId);
        }
        else
        {
            logger.LogError(LogTypeEnum.Business, null,
                "Failed to cancel order {OrderId}: {Error}. CorrelationId: {CorrelationId}",
                @event.OrderId, result.Error, @event.CorrelationId);
        }
    }
}
