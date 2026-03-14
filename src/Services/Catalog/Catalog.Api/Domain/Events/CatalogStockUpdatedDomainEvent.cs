using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using Catalog.Api.Domain.Entities;

namespace Catalog.Api.Domain.Events;

/// <summary>
/// Raised by <see cref="CatalogEntity"/> after a stock mutation
/// (reserve, confirm, or release). Saved atomically to the Outbox and
/// dispatched by <see cref="BuildingBlocks.Messaging.Outbox.OutboxProcessor{TDbContext}"/>.
/// </summary>
public class CatalogStockUpdatedDomainEvent(CatalogEntity catalog, string? correlationId = null) : IDomainEvent
{
    public Guid CatalogId { get; } = catalog.Id.Value;
    public Guid MerchantId { get; } = catalog.MerchantId;
    public string ProductName { get; } = catalog.Name;
    public int Available { get; } = catalog.Stock.Available;
    public int Reserved { get; } = catalog.Stock.Reserved;
    public string? CorrelationId { get; } = correlationId;
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
