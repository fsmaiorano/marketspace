namespace BackendForFrontend.Api.Order.Dtos;

public class CreateOrderRequest
{
    public Guid CustomerId { get; set; }
    public AddressDto ShippingAddress { get; set; } = new();
    public AddressDto BillingAddress { get; set; } = new();
    public PaymentDto Payment { get; set; } = new();
    public List<OrderItemDto> Items { get; set; } = new();
}

public class AddressDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}

public class PaymentDto
{
    public string CardName { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string Expiration { get; set; } = string.Empty;
    public string CVV { get; set; } = string.Empty;
    public int PaymentMethod { get; set; }
}

public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
