using BuildingBlocks.Abstractions;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using Order.Api.Domain.Entities;
using System.Text.Json.Serialization;

namespace Order.Api.Domain.Events;

public class OrderCreatedDomainEvent : IDomainEvent
{
    public OrderEntity Order { get; set; } = null!;
    public string? CorrelationId { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    [JsonConstructor]
    public OrderCreatedDomainEvent(OrderEntity order, string? correlationId, DateTime occurredAt)
    {
        Order = order;
        CorrelationId = correlationId;
        OccurredAt = occurredAt;
    }

    public OrderCreatedDomainEvent(OrderEntity order, string? correlationId = null)
    {
        Order = order;
        CorrelationId = correlationId;
        OccurredAt = DateTime.UtcNow;
    }

    public UniqueEntityId GetAggregateId()
    {
        return new UniqueEntityId(Order.Id.ToString());
    }
}
