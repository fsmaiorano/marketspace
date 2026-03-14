using BuildingBlocks.Abstractions;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using Order.Api.Domain.Entities;
using System.Text.Json.Serialization;

namespace Order.Api.Domain.Events;

public class OrderCreatedDomainEvent(OrderEntity order, string? correlationId = null) : IDomainEvent
{
    public OrderEntity Order { get; } = order;
    public string? CorrelationId { get; } = correlationId;
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    [JsonConstructor]
    public OrderCreatedDomainEvent(OrderEntity order, string? correlationId, DateTime occurredAt)
        : this(order, correlationId)
    {
        OccurredAt = occurredAt;
    }
}
