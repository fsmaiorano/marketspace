using BuildingBlocks.Abstractions;
using Order.Api.Domain.Enums;
using Order.Api.Domain.Events;
using Order.Api.Domain.ValueObjects;
using System.Text.Json.Serialization;

namespace Order.Api.Domain.Entities;

public class OrderEntity : Aggregate<OrderId>
{
    public CustomerId CustomerId { get; private set; } = null!;
    public Address ShippingAddress { get; private set; } = null!;
    public Address BillingAddress { get; private set; } = null!;
    public Payment Payment { get; private set; } = null!;
    public OrderStatusEnum Status { get; private set; } = OrderStatusEnum.Created;
    public List<OrderItemEntity> Items { get; private set; } = [];
    public Price TotalAmount { get; private set; } = Price.Of(0);

    private void AddItem(OrderId orderId, OrderItemEntity item)
    {
        try
        {
            OrderItemEntity? existingItem = Items.FirstOrDefault(i =>
                i.OrderId.Value == orderId.Value && i.CatalogId.Value == item.CatalogId.Value);
            if (existingItem != null)
            {
                UpdateItemQuantity(item.OrderId, existingItem.Quantity + item.Quantity);
            }
            else
            {
                Items.Add(item);
                CalculateAndSetTotalAmount();
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private void ClearItems()
    {
        Items.Clear();
        CalculateAndSetTotalAmount();
    }

    public OrderEntity()
    {
    }

    [JsonConstructor]
    public OrderEntity(OrderId id, CustomerId customerId, Address shippingAddress, Address billingAddress, 
        Payment payment, OrderStatusEnum status, List<OrderItemEntity> items, Price totalAmount)
    {
        Id = id;
        CustomerId = customerId;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        Payment = payment;
        Status = status;
        Items = items ?? [];
        TotalAmount = totalAmount;
    }

    private void UpdateItemQuantity(OrderId orderId, int quantity)
    {
        ArgumentNullException.ThrowIfNull(orderId, nameof(orderId));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

        OrderItemEntity? item = Items.FirstOrDefault(i => i.OrderId.Value == orderId.Value);
        if (item == null)
            throw new InvalidOperationException($"OrderItem with OrderId {orderId.Value} not found.");

        Items.Remove(item);
        OrderItemEntity updatedItem = OrderItemEntity.Update(item.Id, Id, item.CatalogId, quantity, item.Price);
        Items.Add(updatedItem);
        CalculateAndSetTotalAmount();
    }

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

    public static OrderEntity Create(
        OrderId orderId,
        CustomerId customerId,
        Address shippingAddress,
        Address billingAddress,
        Payment payment,
        OrderStatusEnum? status,
        IEnumerable<OrderItemEntity>? items = null,
        string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(orderId, nameof(orderId));
        ArgumentNullException.ThrowIfNull(customerId, nameof(customerId));
        ArgumentNullException.ThrowIfNull(shippingAddress, nameof(shippingAddress));
        ArgumentNullException.ThrowIfNull(billingAddress, nameof(billingAddress));
        ArgumentNullException.ThrowIfNull(payment, nameof(payment));
        ArgumentNullException.ThrowIfNull(items, nameof(items));

        if (customerId.Value == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty.", nameof(customerId));

        OrderEntity order = new()
        {
            Id = orderId,
            CustomerId = customerId,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            Payment = payment,
            Status = status ?? OrderStatusEnum.Created
        };

        foreach (OrderItemEntity orderItem in items)
        {
            order.AddItem(orderId, orderItem);
        }

        order.CalculateAndSetTotalAmount();
        order.AddDomainEvent(new OrderCreatedDomainEvent(order, correlationId));

        return order;
    }

    public void Update(
        Address shippingAddress,
        Address billingAddress,
        Payment payment,
        OrderStatusEnum status,
        IEnumerable<OrderItemEntity>? items = null)
    {
        ArgumentNullException.ThrowIfNull(shippingAddress, nameof(shippingAddress));
        ArgumentNullException.ThrowIfNull(billingAddress, nameof(billingAddress));
        ArgumentNullException.ThrowIfNull(payment, nameof(payment));

        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        Payment = payment;
        Status = status;
        LastModifiedAt = DateTime.UtcNow;

        ClearItems();

        if (items != null)
        {
            foreach (OrderItemEntity orderItem in items)
            {
                AddItem(Id, orderItem);
            }
        }

        CalculateAndSetTotalAmount();
    }

    public void PatchStatus(OrderStatusEnum status)
    {
        Status = status;
        LastModifiedAt = DateTime.UtcNow;
    }
}