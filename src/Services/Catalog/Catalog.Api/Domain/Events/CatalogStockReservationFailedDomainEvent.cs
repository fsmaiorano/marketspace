using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using BuildingBlocks.Messaging.IntegrationEvents;

namespace Catalog.Api.Domain.Events;

/// <summary>
/// Raised when stock reservation fails for an order item, after compensation
/// (release of previously reserved items) has been committed.
/// Saved atomically to the Outbox to guarantee at-least-once delivery of the
/// downstream <see cref="StockReservationFailedIntegrationEvent"/>.
/// </summary>
public class CatalogStockReservationFailedDomainEvent(
    Guid orderId,
    Guid customerId,
    Guid merchantId,
    string productName,
    Guid catalogId,
    int requestedQuantity,
    int availableQuantity,
    string failureReason,
    List<OrderItemData> items,
    string? correlationId = null) : IDomainEvent
{
    public Guid OrderId { get; } = orderId;
    public Guid CustomerId { get; } = customerId;
    public Guid MerchantId { get; } = merchantId;
    public string ProductName { get; } = productName;
    public Guid CatalogId { get; } = catalogId;
    public int RequestedQuantity { get; } = requestedQuantity;
    public int AvailableQuantity { get; } = availableQuantity;
    public string FailureReason { get; } = failureReason;
    public List<OrderItemData> Items { get; } = items;
    public string? CorrelationId { get; } = correlationId;
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
