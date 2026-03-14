using Basket.Api.Domain.Entities;
using Basket.Api.Domain.ValueObjects;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using System.Text.Json.Serialization;

namespace Basket.Api.Domain.Events;

public class BasketCheckoutDomainEvent(ShoppingCartEntity shoppingCart, CheckoutData checkoutData) : IDomainEvent
{
    public ShoppingCartEntity ShoppingCart { get; } = shoppingCart;
    public CheckoutData CheckoutData { get; } = checkoutData;
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    [JsonConstructor]
    public BasketCheckoutDomainEvent(ShoppingCartEntity shoppingCart, CheckoutData checkoutData, DateTime occurredAt)
        : this(shoppingCart, checkoutData)
    {
        OccurredAt = occurredAt;
    }
}
