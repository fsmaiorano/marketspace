using Basket.Api.Domain.Events;
using Basket.Api.Domain.ValueObjects;
using BuildingBlocks.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Basket.Api.Domain.Entities;

public class ShoppingCartEntity : Aggregate<string>
{
    [Key] public string Username { get; set; } = null!;

    public List<ShoppingCartItemEntity> Items { get; set; } = new();

    public decimal TotalPrice => Items.Sum(item => item.Price * item.Quantity);

    [JsonConstructor]
    public ShoppingCartEntity(string username, List<ShoppingCartItemEntity> items)
    {
        Username = username;
        Items = items ?? new();
    }

    public ShoppingCartEntity() { } 

    public static ShoppingCartEntity Create(string username, List<ShoppingCartItemEntity> items)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(username));

        if (items == null || !items.Any())
            throw new ArgumentException("Items cannot be null or empty.", nameof(items));

        return new ShoppingCartEntity { Username = username, Items = items };
    }
    
    public static ShoppingCartEntity Update(ShoppingCartEntity cart, List<ShoppingCartItemEntity> items)
    {
        cart.Items = items;
        return cart;
    }

    public void Checkout(CheckoutData checkoutData)
    {
        AddDomainEvent(new BasketCheckoutDomainEvent(this, checkoutData));
    }
}

