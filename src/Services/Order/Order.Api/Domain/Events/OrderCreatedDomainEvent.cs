using BuildingBlocks.Abstractions;
using BuildingBlocks.Messaging.DomainEvents;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using Order.Api.Domain.Entities;

namespace Order.Api.Domain.Events;

public class OrderCreatedDomainEvent : IDomainEvent
{
    public OrderEntity Order { get; }
    public string? CorrelationId { get; }
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public OrderCreatedDomainEvent(OrderEntity order, string? correlationId = null)
    {
        Order = order;
        CorrelationId = correlationId;
    }

    public UniqueEntityId GetAggregateId()
    {
        return new UniqueEntityId(Order.Id.ToString());
    }
}
