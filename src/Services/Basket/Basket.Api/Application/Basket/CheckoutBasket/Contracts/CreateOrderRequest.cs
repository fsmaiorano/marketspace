namespace Basket.Api.Application.Basket.CheckoutBasket.Contracts;

public class CreateOrderRequest
{
    public Guid CustomerId { get; set; }
    public AddressRequest ShippingAddress { get; set; } = null!;
    public AddressRequest BillingAddress { get; set; } = null!;
    public PaymentRequest Payment { get; set; } = null!;
    public List<OrderItemRequest> Items { get; set; } = [];
}

public class AddressRequest
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string EmailAddress { get; set; } = null!;
    public string AddressLine { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string State { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
}

public class PaymentRequest
{
    public string CardNumber { get; set; } = null!;
    public string CardName { get; set; } = null!;
    public string Expiration { get; set; } = null!;
    public string Cvv { get; set; } = null!;
    public int PaymentMethod { get; set; }
}

public class OrderItemRequest
{
    public Guid CatalogId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

