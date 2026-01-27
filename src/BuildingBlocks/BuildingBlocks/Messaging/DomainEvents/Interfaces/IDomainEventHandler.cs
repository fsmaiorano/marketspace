namespace BuildingBlocks.Messaging.DomainEvents.Interfaces;

/// <summary>
/// Interface for handling domain events within the same bounded context
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}