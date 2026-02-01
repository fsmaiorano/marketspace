using BuildingBlocks.Abstractions;

namespace BuildingBlocks.Messaging.DomainEvents.Interfaces;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
    UniqueEntityId GetAggregateId();
}