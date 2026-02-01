namespace BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

/// <summary>
/// Base interface for integration events (cross-service events)
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
    string? CorrelationId { get; }
}

