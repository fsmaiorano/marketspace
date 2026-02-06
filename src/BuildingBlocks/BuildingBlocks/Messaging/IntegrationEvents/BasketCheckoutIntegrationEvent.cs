using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

namespace BuildingBlocks.Messaging.IntegrationEvents;

public class BasketCheckoutIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }

    public Guid CustomerId { get; set; }
    public string UserName { get; set; } = null!;
    public AddressData ShippingAddress { get; set; } = null!;
    public AddressData BillingAddress { get; set; } = null!;
    public PaymentData Payment { get; set; } = null!;
    public List<OrderItemData> Items { get; set; } = [];
    public decimal TotalPrice { get; set; }
}

public class AddressData
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string EmailAddress { get; set; } = null!;
    public string AddressLine { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string State { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
    public string Coordinates { get; set; } = null!;
}

public class PaymentData
{
    public string CardNumber { get; set; } = null!;
    public string CardName { get; set; } = null!;
    public string Expiration { get; set; } = null!;
    public string Cvv { get; set; } = null!;
    public int PaymentMethod { get; set; }
}

public class OrderItemData
{
    public Guid CatalogId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}