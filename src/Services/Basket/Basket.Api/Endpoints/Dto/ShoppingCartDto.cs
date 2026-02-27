namespace Basket.Api.Endpoints.Dto;

public class ShoppingCartDto
{
    public required string Username { get; init; }
    public List<ShoppingCartItemDto> Items { get; init; } = [];
    public decimal TotalPrice => Items.Sum(item => item.Price * item.Quantity);

    public ShoppingCartDto()
    {
    }
}