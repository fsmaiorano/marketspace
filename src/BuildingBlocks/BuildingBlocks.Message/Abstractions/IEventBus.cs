namespace BuildingBlocks.Message.Abstractions;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, string topic, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;

    Task SubscribeAsync<TEvent, THandler>(string topic, string subscription, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
        where THandler : IEventHandler<TEvent>;
}
