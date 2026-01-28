using BuildingBlocks.Abstractions;

namespace BuildingBlocks.Messaging.DomainEvents;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
    UniqueEntityId GetAggregateId();
}