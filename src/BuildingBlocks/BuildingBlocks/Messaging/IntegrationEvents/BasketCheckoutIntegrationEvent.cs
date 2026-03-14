namespace BuildingBlocks.Messaging.IntegrationEvents;

public class BasketCheckoutIntegrationEvent : IntegrationEvent
{
    public Guid CustomerId { get; set; }
    public string UserName { get; set; } = null!;
    public AddressData ShippingAddress { get; set; } = null!;
    public AddressData BillingAddress { get; set; } = null!;
    public PaymentData Payment { get; set; } = null!;
    public List<OrderItemData> Items { get; set; } = [];
    public decimal TotalPrice { get; set; }
}