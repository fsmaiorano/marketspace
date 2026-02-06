using Basket.Api.Domain.Entities;
using Basket.Api.Domain.ValueObjects;
using BuildingBlocks.Abstractions;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using System.Text.Json.Serialization;

namespace Basket.Api.Domain.Events;

public class BasketCheckoutDomainEvent : IDomainEvent
{
    public ShoppingCartEntity ShoppingCart { get; set; } = null!;
    public CheckoutData CheckoutData { get; set; } = null!;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    [JsonConstructor]
    public BasketCheckoutDomainEvent(ShoppingCartEntity shoppingCart, CheckoutData checkoutData, DateTime occurredAt)
    {
        ShoppingCart = shoppingCart;
        CheckoutData = checkoutData;
        OccurredAt = occurredAt;
    }

    public BasketCheckoutDomainEvent(ShoppingCartEntity shoppingCart, CheckoutData checkoutData)
    {
        ShoppingCart = shoppingCart;
        CheckoutData = checkoutData;
        OccurredAt = DateTime.UtcNow;
    }

    public UniqueEntityId GetAggregateId()
    {
        return new UniqueEntityId(ShoppingCart.Id);
    }
}
