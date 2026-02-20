namespace Basket.Api.Application.Dto;

public class ShoppingCartDto
{
    public string Username { get; init; }
    public List<ShoppingCartItemDto> Items { get; init; } = [];
    public decimal TotalPrice => Items.Sum(item => item.Price * item.Quantity);

    public ShoppingCartDto()
    {
    }
}