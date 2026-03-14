using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.Interfaces;
using Catalog.Api.Domain.Events;

namespace Catalog.Api.Application.EventHandlers;

/// <summary>
/// Publishes a <see cref="StockReservationFailedIntegrationEvent"/> when stock
/// reservation fails for an order item in the Catalog service.
/// Invoked by the <see cref="BuildingBlocks.Messaging.Outbox.OutboxProcessor{TDbContext}"/>,
/// ensuring that Order, Payment and BFF are always notified even if RabbitMQ
/// was temporarily unavailable at the time the failure was detected.
/// </summary>
public class OnCatalogStockReservationFailedDomainEventHandler(
    IAppLogger<OnCatalogStockReservationFailedDomainEventHandler> logger,
    IEventBus eventBus)
    : IDomainEventHandler<CatalogStockReservationFailedDomainEvent>
{
    public async Task HandleAsync(CatalogStockReservationFailedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Application,
            "Publishing StockReservationFailedIntegrationEvent. OrderId: {OrderId}, CatalogId: {CatalogId}, Reason: {Reason}, CorrelationId: {CorrelationId}",
            @event.OrderId, @event.CatalogId, @event.FailureReason, @event.CorrelationId);

        await eventBus.PublishAsync(new StockReservationFailedIntegrationEvent
        {
            OrderId = @event.OrderId,
            CustomerId = @event.CustomerId,
            MerchantId = @event.MerchantId,
            ProductName = @event.ProductName,
            CatalogId = @event.CatalogId,
            RequestedQuantity = @event.RequestedQuantity,
            AvailableQuantity = @event.AvailableQuantity,
            FailureReason = @event.FailureReason,
            Items = @event.Items,
            CorrelationId = @event.CorrelationId
        }, cancellationToken);
    }
}
