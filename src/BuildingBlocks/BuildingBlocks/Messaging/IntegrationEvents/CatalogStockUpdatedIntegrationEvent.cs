using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

namespace BuildingBlocks.Messaging.IntegrationEvents;

public class CatalogStockUpdatedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }

    public Guid CatalogId { get; set; }
    public Guid MerchantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int NewStock { get; set; }
}
