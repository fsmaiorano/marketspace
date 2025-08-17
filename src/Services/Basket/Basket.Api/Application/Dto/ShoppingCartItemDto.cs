namespace Basket.Api.Application.Dto;

public class ShoppingCartItemDto
{
    public int Quantity { get; set; } = 0!;

    public decimal Price { get; set; } = 0m!;

    // public Guid ProductId { get; set; } = Guid.Empty!;
    public string ProductName { get; set; } = null!;
}