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
    public Price TotalAmount { get; private set; } = Price.Of(0);

    private void CalculateAndSetTotalAmount()
    {
        if (Items.Count == 0)
        {
            TotalAmount = Price.Of(0);
            return;
        }

        decimal total = Items.Sum(item => item.Price.Value * item.Quantity);
        TotalAmount = Price.Of(total);
    }

    public OrderEntity()
    {
        
    }
    
    public static OrderEntity Create(
        OrderId orderId,
        CustomerId customerId,
        Address shippingAddress,
        Address billingAddress,
        Payment payment,
        List<OrderItemEntity> items)
    {
        ArgumentNullException.ThrowIfNull(orderId, nameof(orderId));
        ArgumentNullException.ThrowIfNull(customerId, nameof(customerId));
        ArgumentNullException.ThrowIfNull(shippingAddress, nameof(shippingAddress));
        ArgumentNullException.ThrowIfNull(billingAddress, nameof(billingAddress));
        ArgumentNullException.ThrowIfNull(payment, nameof(payment));
        ArgumentNullException.ThrowIfNull(items, nameof(items));

        if (orderId.Value == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty.", nameof(orderId));
        if (customerId.Value == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty.", nameof(customerId));

        var order = new OrderEntity
        {
            Id = orderId,
            CustomerId = customerId,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            Payment = payment,
            Items = items
        };

        order.CalculateAndSetTotalAmount();
        return order;
    }
}