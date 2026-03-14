namespace BuildingBlocks.Messaging.IntegrationEvents;

public class OrderItemData
{
    public Guid CatalogId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
