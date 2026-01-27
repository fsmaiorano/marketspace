using BuildingBlocks.Abstractions;
using BuildingBlocks.Messaging.DomainEvents;
using Order.Api.Domain.Entities;

namespace Order.Api.Domain.Events;

public class OrderCreatedDomainEvent(OrderEntity Order) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public UniqueEntityId GetAggregateId()
    {
        return new UniqueEntityId(Order.Id.ToString());
    }
}

//TODO - refatorar para enviar um IntegrationEvent! nao enviar uma entidade