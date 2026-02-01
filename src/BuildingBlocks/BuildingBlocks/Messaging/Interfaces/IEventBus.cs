using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

namespace BuildingBlocks.Messaging.Interfaces;

/// <summary>
/// Interface for publishing and subscribing to integration events across services
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publish an integration event to all subscribers
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
        where TEvent : class, IIntegrationEvent;
    
    /// <summary>
    /// Subscribe to integration events of a specific type
    /// </summary>
    void Subscribe<TEvent, THandler>()
        where TEvent : class, IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>;
}

