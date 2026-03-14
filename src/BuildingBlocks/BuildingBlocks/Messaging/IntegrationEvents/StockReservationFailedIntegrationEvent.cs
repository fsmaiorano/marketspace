using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

namespace BuildingBlocks.Messaging.IntegrationEvents;

/// <summary>
/// Published by the Catalog service when stock reservation fails for one or more items in an order.
/// Triggers order cancellation (Order service), payment cancellation (Payment service),
/// and merchant notification (BFF).
/// </summary>
public class StockReservationFailedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }

    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }

    /// <summary>MerchantId of the first catalog item that caused the failure.</summary>
    public Guid MerchantId { get; init; }

    /// <summary>Name of the product whose reservation failed.</summary>
    public string ProductName { get; init; } = string.Empty;

    public Guid CatalogId { get; init; }
    public int RequestedQuantity { get; init; }
    public int AvailableQuantity { get; init; }
    public string FailureReason { get; init; } = string.Empty;

    /// <summary>All items in the order — used by Order/Payment services for context.</summary>
    public List<OrderItemData> Items { get; init; } = [];
}
