namespace BuildingBlocks.Message.Abstractions;

public interface IEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
