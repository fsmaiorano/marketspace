using Basket.Api.Domain.Entities;
using Basket.Api.Domain.ValueObjects;
using BuildingBlocks.Abstractions;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;

namespace Basket.Api.Domain.Events;

public class BasketCheckoutDomainEvent : IDomainEvent
{
    public ShoppingCartEntity ShoppingCart { get; }
    public CheckoutData CheckoutData { get; }
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public BasketCheckoutDomainEvent(ShoppingCartEntity shoppingCart, CheckoutData checkoutData)
    {
        ShoppingCart = shoppingCart;
        CheckoutData = checkoutData;
    }

    public UniqueEntityId GetAggregateId()
    {
        return new UniqueEntityId(ShoppingCart.Id);
    }
}
