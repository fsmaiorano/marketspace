using Order.Api.Application.Dto;
using Order.Api.Domain.Enums;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Application.Order.UpdateOrder;

public class UpdateOrderCommand
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid CustomerId { get; set; } = Guid.Empty;
    public AddressDto ShippingAddress { get; set; } = null!;
    public AddressDto BillingAddress { get; set; } = null!;
    public PaymentDto Payment { get; set; } = null!;
    public OrderStatusEnum Status { get; set; } = OrderStatusEnum.Pending;
    public List<OrderItemDto> Items { get; set; } = [];
    public Price TotalAmount { get; set; } = Price.Of(0);
}