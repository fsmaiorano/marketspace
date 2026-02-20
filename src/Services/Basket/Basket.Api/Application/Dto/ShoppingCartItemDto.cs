namespace Basket.Api.Application.Dto;

public class ShoppingCartItemDto
{
    public int Quantity { get; init; }
    public decimal Price { get; init; }
    public string ProductId { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
}