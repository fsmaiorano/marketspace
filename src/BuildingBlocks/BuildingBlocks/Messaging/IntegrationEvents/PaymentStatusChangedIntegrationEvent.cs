namespace BuildingBlocks.Messaging.IntegrationEvents;

/// <summary>
/// Integration event to notify when payment status changes.
/// Uses primitive types to avoid coupling between services.
/// See <see cref="PaymentStatusCodes"/> for named status values.
/// </summary>
public class PaymentStatusChangedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }

    /// <summary>
    /// Payment status as integer. See <see cref="PaymentStatusCodes"/> for named constants.
    /// </summary>
    public int PaymentStatus { get; set; }

    public string? PaymentStatusName { get; set; }

    /// <summary>
    /// Order items carried from the original order, used by downstream services (e.g. Catalog) to
    /// restore reserved stock when payment fails.
    /// </summary>
    public List<OrderItemData> Items { get; init; } = [];
}