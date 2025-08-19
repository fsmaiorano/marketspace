using Order.Api.Application.Dto;
using Order.Api.Domain.Enums;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Application.Order.CreateOrder;

public class CreateOrderCommand
{
    public Guid CustomerId { get; private set; } = Guid.Empty;
    public AddressDto ShippingAddress { get; private set; } = null!;
    public AddressDto BillingAddress { get; private set; } = null!;
    public PaymentDto Payment { get; private set; } = null!;
    public OrderStatusEnum Status { get; private set; } = OrderStatusEnum.Pending;
    public List<OrderItemDto> Items { get; private set; } = [];
    public Price TotalAmount { get; private set; } = Price.Of(0);
}