using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

namespace BuildingBlocks.Messaging.IntegrationEvents;

public class PaymentStatusChangedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }
    public Guid OrderId { get; set; }
    public bool IsProcessing { get; set; }
    public bool IsAuthorized { get; set; }
}