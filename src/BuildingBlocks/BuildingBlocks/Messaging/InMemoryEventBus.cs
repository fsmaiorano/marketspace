using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging;

/// <summary>
/// In-memory implementation of event bus for testing or single-instance scenarios
/// </summary>
public class InMemoryEventBus(IServiceProvider serviceProvider, ILogger<InMemoryEventBus> logger) : IEventBus
{
    private readonly Dictionary<Type, List<Type>> _handlers = new();

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent
    {
        var eventType = @event.GetType();

        if (!_handlers.TryGetValue(eventType, out var handlerTypes))
        {
            logger.LogDebug("No handlers registered for event type {EventType}", eventType.Name);
            return;
        }

        logger.LogInformation("Publishing integration event {EventType} with EventId {EventId}", 
            eventType.Name, @event.EventId);

        using var scope = serviceProvider.CreateScope();
        var tasks = new List<Task>();

        foreach (var handlerType in handlerTypes)
        {
            var handler = scope.ServiceProvider.GetService(handlerType);
            if (handler is null)
            {
                logger.LogWarning("Handler {HandlerType} not found in service provider", handlerType.Name);
                continue;
            }

            var method = handlerType.GetMethod(nameof(IIntegrationEventHandler<TEvent>.HandleAsync));
            var task = (Task?)method?.Invoke(handler, [@event, cancellationToken]);
            
            if (task != null)
                tasks.Add(task);
        }

        if (tasks.Count > 0)
            await Task.WhenAll(tasks);
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : class, IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);

        if (!_handlers.ContainsKey(eventType))
            _handlers[eventType] = new List<Type>();

        if (!_handlers[eventType].Contains(handlerType))
        {
            _handlers[eventType].Add(handlerType);
            logger.LogInformation("Subscribed {HandlerType} to {EventType}", handlerType.Name, eventType.Name);
        }
    }
}