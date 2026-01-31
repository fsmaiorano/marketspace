using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

namespace BaseTest.Mocks;

public class TestEventBus : IEventBus
{
    // Simple in-memory no-op event bus for tests
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class, IIntegrationEvent
    {
        // No-op: do not forward to RabbitMQ during tests
        return Task.CompletedTask;
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : class, IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        // No-op: tests can register handlers directly if needed
    }
}
