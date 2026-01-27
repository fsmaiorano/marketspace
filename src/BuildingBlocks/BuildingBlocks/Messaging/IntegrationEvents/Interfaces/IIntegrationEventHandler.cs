namespace BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

/// <summary>
/// Interface for handling integration events (cross-service communication)
/// </summary>
public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}