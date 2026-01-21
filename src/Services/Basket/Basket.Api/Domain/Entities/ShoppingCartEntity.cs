using System.ComponentModel.DataAnnotations;

namespace Basket.Api.Domain.Entities;

public class ShoppingCartEntity
{
    [Key]
    public string Username { get; set; } = null!;

    public List<ShoppingCartItemEntity> Items { get; set; } = new();

    public decimal TotalPrice => Items.Sum(item => item.Price * item.Quantity);

    public static ShoppingCartEntity Create(string username, List<ShoppingCartItemEntity> items)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(username));

        if (items == null || !items.Any())
            throw new ArgumentException("Items cannot be null or empty.", nameof(items));

        return new ShoppingCartEntity
        {
            Username = username,
            Items = items
        };
    }
}