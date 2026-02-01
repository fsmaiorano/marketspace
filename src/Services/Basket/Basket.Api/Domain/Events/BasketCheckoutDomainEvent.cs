using Basket.Api.Domain.Entities;
using BuildingBlocks.Abstractions;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;

namespace Basket.Api.Domain.Events;

public class BasketCheckoutDomainEvent(ShoppingCartEntity ShoppingCart) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public UniqueEntityId GetAggregateId()
    {
        return new UniqueEntityId(ShoppingCart.Id.ToString());
    }
}
