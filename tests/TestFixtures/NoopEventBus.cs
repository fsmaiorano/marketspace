using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

namespace MarketSpace.TestFixtures;

public class NoopEventBus : IEventBus
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class, IIntegrationEvent
    {
        return Task.CompletedTask;
    }

    public void Subscribe<TEvent, THandler>() where TEvent : class, IIntegrationEvent where THandler : IIntegrationEventHandler<TEvent>
    {
    }
}