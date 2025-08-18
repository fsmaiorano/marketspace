using BuildingBlocks.Abstractions;
using Order.Api.Domain.Enums;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Domain.Entities;

public class OrderEntity : Aggregate<OrderId>
{
    public CustomerId CustomerId { get; private set; } = null!;
    public Address ShippingAddress { get; private set; } = null!;
    public Address BillingAddress { get; private set; } = null!;
    public Payment Payment { get; private set; } = null!;
    public OrderStatusEnum Status { get; private set; } = OrderStatusEnum.Pending;
    public List<OrderItemEntity> Items { get; private set; } = [];
}