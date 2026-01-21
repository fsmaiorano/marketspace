namespace Basket.Api.Domain.Entities;

public class ShoppingCartItemEntity
{
    public string ProductId { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string ProductName { get; set; } = null!;
}