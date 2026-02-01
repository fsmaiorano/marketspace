using Basket.Api.Application.Basket.CheckoutBasket.Dtos;
using Basket.Api.Domain.Entities;
using BuildingBlocks.Abstractions;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;

namespace Basket.Api.Domain.Events;

public class BasketCheckoutDomainEvent : IDomainEvent
{
    public ShoppingCartEntity ShoppingCart { get; }
    public CheckoutDataDto CheckoutData { get; }
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public BasketCheckoutDomainEvent(ShoppingCartEntity shoppingCart, CheckoutDataDto checkoutData)
    {
        ShoppingCart = shoppingCart;
        CheckoutData = checkoutData;
    }

    public UniqueEntityId GetAggregateId()
    {
        return new UniqueEntityId(ShoppingCart.Id);
    }
}
