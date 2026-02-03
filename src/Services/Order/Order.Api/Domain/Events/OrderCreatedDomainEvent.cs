using BuildingBlocks.Abstractions;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using Order.Api.Domain.Entities;

namespace Order.Api.Domain.Events;

public class OrderCreatedDomainEvent(OrderEntity order, string? correlationId = null) : IDomainEvent
{
    public OrderEntity Order { get; } = order;
    public string? CorrelationId { get; } = correlationId;
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public UniqueEntityId GetAggregateId()
    {
        return new UniqueEntityId(Order.Id.ToString());
    }
}
