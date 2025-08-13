namespace Merchant.Api.Domain.Abstractions;

public interface IAggregate<T> : IAggregate, IEntity<T>
{
}

public interface IAggregate : IEntity
{
    IReadOnlyList<object> DomainEvents { get; }
    object[] ClearDomainEvents();
}

public abstract class Aggregate<TId> : Entity<TId>, IAggregate<TId>
    where TId : notnull
{
    private readonly List<object> _domainEvents = [];

    public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(object domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    public object[] ClearDomainEvents()
    {
        object[] events = _domainEvents.ToArray();
        _domainEvents.Clear();
        return events;
    }
}