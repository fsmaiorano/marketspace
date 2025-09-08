namespace Basket.Api.Application.Dto;

public class ShoppingCartItemDto
{
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
}