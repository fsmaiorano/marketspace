using System.Text.Json.Serialization;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using BuildingBlocks.Messaging.IntegrationEvents;

namespace Catalog.Api.Domain.Events;

/// <summary>
/// Raised when stock reservation fails for an order item, after compensation
/// (release of previously reserved items) has been committed.
/// Saved atomically to the Outbox to guarantee at-least-once delivery of the
/// downstream <see cref="StockReservationFailedIntegrationEvent"/>.
/// </summary>
public class CatalogStockReservationFailedDomainEvent : IDomainEvent
{
    public CatalogStockReservationFailedDomainEvent(
        Guid orderId, Guid customerId, Guid merchantId,
        string productName, Guid catalogId,
        int requestedQuantity, int availableQuantity,
        string failureReason, List<OrderItemData> items,
        string? correlationId = null)
    {
        OrderId = orderId;
        CustomerId = customerId;
        MerchantId = merchantId;
        ProductName = productName;
        CatalogId = catalogId;
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
        FailureReason = failureReason;
        Items = items;
        CorrelationId = correlationId;
        OccurredAt = DateTime.UtcNow;
    }

    [JsonConstructor]
    public CatalogStockReservationFailedDomainEvent(
        Guid orderId, Guid customerId, Guid merchantId,
        string productName, Guid catalogId,
        int requestedQuantity, int availableQuantity,
        string failureReason, List<OrderItemData> items,
        string? correlationId, DateTime occurredAt)
    {
        OrderId = orderId;
        CustomerId = customerId;
        MerchantId = merchantId;
        ProductName = productName;
        CatalogId = catalogId;
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
        FailureReason = failureReason;
        Items = items;
        CorrelationId = correlationId;
        OccurredAt = occurredAt;
    }

    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public Guid MerchantId { get; }
    public string ProductName { get; }
    public Guid CatalogId { get; }
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }
    public string FailureReason { get; }
    public List<OrderItemData> Items { get; }
    public string? CorrelationId { get; }
    public DateTime OccurredAt { get; }
}
