namespace Order.Api.Endpoints.Dto;

public class OrderDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public AddressDto ShippingAddress { get; init; } = null!;
    public AddressDto BillingAddress { get; init; } = null!;
    public PaymentDto Payment { get; init; } = null!;
    public string? Status { get; init; }
    public List<OrderItemDto> Items { get; init; } = [];
    public decimal TotalAmount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}