using BuildingBlocks.Abstractions;
using Order.Api.Domain.ValueObjects;
using System.Text.Json.Serialization;

namespace Order.Api.Domain.Entities;

public class OrderItemEntity : Aggregate<OrderItemId>
{
    public OrderId OrderId { get; private set; } = null!;
    public CatalogId CatalogId { get; private set; } = null!; //ProductId - TODO - Refactory
    public int Quantity { get; private set; } = 0;
    public Price Price { get; private set; } = null!;

    public OrderItemEntity() { } 

    [JsonConstructor]
    public OrderItemEntity(OrderItemId id, OrderId orderId, CatalogId catalogId, int quantity, Price price)
    {
        Id = id;
        OrderId = orderId;
        CatalogId = catalogId;
        Quantity = quantity;
        Price = price;
    }

    public static OrderItemEntity Create(
        OrderId? orderId,
        CatalogId catalogId,
        int quantity,
        Price price)
    {
        ArgumentNullException.ThrowIfNull(orderId, nameof(orderId));
        ArgumentNullException.ThrowIfNull(catalogId, nameof(catalogId));
        
        if (catalogId.Value == Guid.Empty)
            throw new ArgumentException("CatalogId cannot be empty.", nameof(catalogId));
        
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        
        ArgumentNullException.ThrowIfNull(price);

        return new OrderItemEntity
        {
            Id = OrderItemId.Of(Guid.CreateVersion7()),
            OrderId = orderId,
            CatalogId = catalogId,
            Quantity = quantity,
            Price = price
        };
    }

    public static OrderItemEntity Update(
        OrderItemId id,
        OrderId orderId,
        CatalogId catalogId,
        int quantity,
        Price price)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        ArgumentNullException.ThrowIfNull(orderId, nameof(orderId));
        ArgumentNullException.ThrowIfNull(catalogId, nameof(catalogId));
        if (orderId.Value == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty.", nameof(orderId));
        if (catalogId.Value == Guid.Empty)
            throw new ArgumentException("CatalogId cannot be empty.", nameof(catalogId));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        if (price == null) throw new ArgumentNullException(nameof(price));

        return new OrderItemEntity
        {
            Id = id,
            OrderId = orderId,
            CatalogId = catalogId,
            Quantity = quantity,
            Price = price
        };
    }
}