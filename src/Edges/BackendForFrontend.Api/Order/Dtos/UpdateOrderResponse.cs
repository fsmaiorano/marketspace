namespace BackendForFrontend.Api.Order.Dtos;

public class UpdateOrderResponse
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string OrderName { get; set; } = string.Empty;
    public AddressDto ShippingAddress { get; set; } = new();
    public AddressDto BillingAddress { get; set; } = new();
    public PaymentDto Payment { get; set; } = new();
    public int Status { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
}
