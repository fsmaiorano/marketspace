using BuildingBlocks.Abstractions;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Domain.Entities;

public class OrderItemEntity : Aggregate<OrderItemId>
{
    public OrderId OrderId { get; private set; } = null!;
    public CatalogId CatalogId { get; private set; } = null!;
    public int Quantity { get; private set; } = 0;
    public Price Price { get; private set; }

    public static OrderItemEntity Create(
        OrderItemId orderItemId,
        OrderId orderId,
        CatalogId catalogId,
        int quantity,
        Price price)
    {
        ArgumentNullException.ThrowIfNull(orderItemId, nameof(orderItemId));
        ArgumentNullException.ThrowIfNull(orderId, nameof(orderId));
        ArgumentNullException.ThrowIfNull(catalogId, nameof(catalogId));
        if (orderItemId.Value == Guid.Empty)
            throw new ArgumentException("OrderItemId cannot be empty.", nameof(orderItemId));
        if (orderId.Value == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty.", nameof(orderId));
        if (catalogId.Value == Guid.Empty)
            throw new ArgumentException("CatalogId cannot be empty.", nameof(catalogId));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        if (price == null) throw new ArgumentNullException(nameof(price));

        return new OrderItemEntity
        {
            Id = orderItemId,
            OrderId = orderId,
            CatalogId = catalogId,
            Quantity = quantity,
            Price = price
        };
    }
}