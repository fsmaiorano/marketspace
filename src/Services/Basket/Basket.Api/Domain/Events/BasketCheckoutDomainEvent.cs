using System.Text.Json.Serialization;
using Basket.Api.Domain.Entities;
using Basket.Api.Domain.ValueObjects;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using BuildingBlocks.Messaging.IntegrationEvents;

namespace Basket.Api.Domain.Events;

public class BasketCheckoutDomainEvent : IDomainEvent
{
    public BasketCheckoutDomainEvent(ShoppingCartEntity shoppingCart, CheckoutData checkoutData)
    {
        CheckoutData = checkoutData;
        TotalPrice = shoppingCart.TotalPrice;
        Items = shoppingCart.Items
            .Select(i => new OrderItemData
            {
                CatalogId = Guid.Parse(i.ProductId),
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();
        OccurredAt = DateTime.UtcNow;
    }

    [JsonConstructor]
    public BasketCheckoutDomainEvent(
        CheckoutData checkoutData, decimal totalPrice, List<OrderItemData> items, DateTime occurredAt)
    {
        CheckoutData = checkoutData;
        TotalPrice = totalPrice;
        Items = items;
        OccurredAt = occurredAt;
    }

    public CheckoutData CheckoutData { get; }
    public decimal TotalPrice { get; }
    public List<OrderItemData> Items { get; }
    public DateTime OccurredAt { get; }
}
