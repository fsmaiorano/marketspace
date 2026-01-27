using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.Messaging.DomainEvents;

/// <summary>
/// Service for dispatching domain events to registered handlers
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of domain event dispatcher using service provider
/// </summary>
public class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Type eventType = domainEvent.GetType();
        Type handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

        using IServiceScope scope = serviceProvider.CreateScope();
        IEnumerable<object?> handlers = scope.ServiceProvider.GetServices(handlerType);

        List<Task> tasks = [];
        foreach (object? handler in handlers)
        {
            MethodInfo? method = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync));
            Task? task = (Task?)method?.Invoke(handler, [domainEvent, cancellationToken]);
            if (task != null)
                tasks.Add(task);
        }

        if (tasks.Count > 0)
            await Task.WhenAll(tasks);
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
            await DispatchAsync(domainEvent, cancellationToken);
    }
}