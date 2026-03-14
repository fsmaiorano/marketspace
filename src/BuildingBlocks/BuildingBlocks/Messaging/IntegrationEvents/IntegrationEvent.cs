using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

namespace BuildingBlocks.Messaging.IntegrationEvents;

/// <summary>
/// Base class for all integration events. Provides common metadata fields
/// so concrete events only declare their own domain-specific properties.
/// </summary>
public abstract class IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }
}
