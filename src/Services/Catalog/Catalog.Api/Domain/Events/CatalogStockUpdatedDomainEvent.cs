using System.Text.Json.Serialization;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using Catalog.Api.Domain.Entities;

namespace Catalog.Api.Domain.Events;

/// <summary>
/// Raised by <see cref="CatalogEntity"/> after a stock mutation
/// (reserve, confirm, or release). Saved atomically to the Outbox and
/// dispatched by <see cref="BuildingBlocks.Messaging.Outbox.OutboxProcessor{TDbContext}"/>.
/// </summary>
public class CatalogStockUpdatedDomainEvent : IDomainEvent
{
    public CatalogStockUpdatedDomainEvent(CatalogEntity catalog, string? correlationId = null)
    {
        CatalogId = catalog.Id.Value;
        MerchantId = catalog.MerchantId;
        ProductName = catalog.Name;
        Available = catalog.Stock.Available;
        Reserved = catalog.Stock.Reserved;
        CorrelationId = correlationId;
        OccurredAt = DateTime.UtcNow;
    }

    [JsonConstructor]
    public CatalogStockUpdatedDomainEvent(
        Guid catalogId, Guid merchantId, string productName,
        int available, int reserved, string? correlationId, DateTime occurredAt)
    {
        CatalogId = catalogId;
        MerchantId = merchantId;
        ProductName = productName;
        Available = available;
        Reserved = reserved;
        CorrelationId = correlationId;
        OccurredAt = occurredAt;
    }

    public Guid CatalogId { get; }
    public Guid MerchantId { get; }
    public string ProductName { get; }
    public int Available { get; }
    public int Reserved { get; }
    public string? CorrelationId { get; }
    public DateTime OccurredAt { get; }
}
