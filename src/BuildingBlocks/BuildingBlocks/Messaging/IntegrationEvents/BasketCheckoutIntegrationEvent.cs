using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

namespace BuildingBlocks.Messaging.IntegrationEvents;

public class BasketCheckoutIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }
    
    // Order Data
    public Guid CustomerId { get; init; }
    public string UserName { get; init; } = null!;
    public AddressData ShippingAddress { get; init; } = null!;
    public AddressData BillingAddress { get; init; } = null!;
    public PaymentData Payment { get; init; } = null!;
    public List<OrderItemData> Items { get; init; } = [];
    public decimal TotalPrice { get; init; }
}

public class AddressData
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string EmailAddress { get; init; } = null!;
    public string AddressLine { get; init; } = null!;
    public string Country { get; init; } = null!;
    public string State { get; init; } = null!;
    public string ZipCode { get; init; } = null!;
}

public class PaymentData
{
    public string CardNumber { get; init; } = null!;
    public string CardName { get; init; } = null!;
    public string Expiration { get; init; } = null!;
    public string Cvv { get; init; } = null!;
    public int PaymentMethod { get; init; }
}

public class OrderItemData
{
    public Guid CatalogId { get; init; }
    public int Quantity { get; init; }
    public decimal Price { get; init; }
}