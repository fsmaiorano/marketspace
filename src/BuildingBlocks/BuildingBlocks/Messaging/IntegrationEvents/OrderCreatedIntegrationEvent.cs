namespace BuildingBlocks.Messaging.IntegrationEvents;

public class OrderCreatedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CardNumber { get; init; } = null!;
    public string CardName { get; init; } = null!;
    public string Expiration { get; init; } = null!;
    public string Cvv { get; init; } = null!;
    public int PaymentMethod { get; set; }
    public decimal TotalAmount { get; init; }
    public List<OrderItemData> Items { get; init; } = [];
}