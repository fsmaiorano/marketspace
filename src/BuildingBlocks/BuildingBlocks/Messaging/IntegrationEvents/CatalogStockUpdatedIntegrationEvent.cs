namespace BuildingBlocks.Messaging.IntegrationEvents;

public class CatalogStockUpdatedIntegrationEvent : IntegrationEvent
{
    public Guid CatalogId { get; set; }
    public Guid MerchantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Available { get; set; }
    public int Reserved { get; set; }

    /// <summary>Total available stock — kept for backward compatibility with existing consumers.</summary>
    public int NewStock => Available;
}
