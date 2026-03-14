using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.Interfaces;
using Catalog.Api.Domain.Events;

namespace Catalog.Api.Application.EventHandlers;

/// <summary>
/// Publishes a <see cref="CatalogStockUpdatedIntegrationEvent"/> whenever stock
/// changes in the Catalog service (reserve, confirm, release).
/// Invoked by the <see cref="BuildingBlocks.Messaging.Outbox.OutboxProcessor{TDbContext}"/>,
/// guaranteeing at-least-once delivery even if RabbitMQ is temporarily unavailable.
/// </summary>
public class OnCatalogStockUpdatedDomainEventHandler(
    IAppLogger<OnCatalogStockUpdatedDomainEventHandler> logger,
    IEventBus eventBus)
    : IDomainEventHandler<CatalogStockUpdatedDomainEvent>
{
    public async Task HandleAsync(CatalogStockUpdatedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Application,
            "Publishing CatalogStockUpdatedIntegrationEvent. CatalogId: {CatalogId}, Available: {Available}, Reserved: {Reserved}, CorrelationId: {CorrelationId}",
            @event.CatalogId, @event.Available, @event.Reserved, @event.CorrelationId);

        await eventBus.PublishAsync(new CatalogStockUpdatedIntegrationEvent
        {
            CatalogId = @event.CatalogId,
            MerchantId = @event.MerchantId,
            ProductName = @event.ProductName,
            Available = @event.Available,
            Reserved = @event.Reserved,
            CorrelationId = @event.CorrelationId
        }, cancellationToken);
    }
}
