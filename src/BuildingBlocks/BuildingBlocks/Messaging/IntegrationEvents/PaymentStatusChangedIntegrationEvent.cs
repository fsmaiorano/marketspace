using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

namespace BuildingBlocks.Messaging.IntegrationEvents;

/// <summary>
/// Integration event to notify when payment status changes.
/// Uses primitive types to avoid coupling between services.
/// </summary>
public class PaymentStatusChangedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }
    public Guid OrderId { get; set; }
    
    /// <summary>
    /// Payment status as integer value to avoid coupling between services.
    /// Values: 1=Created, 2=Processing, 3=Authorized, 4=Captured, 5=Failed, 6=Rejected, 7=Cancelled, 8=Refunded, 9=Chargeback
    /// </summary>
    public int PaymentStatus { get; set; }
    
    /// <summary>
    /// Additional payment details for logging/tracking purposes
    /// </summary>
    public string? PaymentStatusName { get; set; }
}