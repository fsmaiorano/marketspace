namespace Basket.Api.Application.Dto;

public class ShoppingCartDto
{
    public string Username { get; set; } = default!;
    public List<ShoppingCartItemDto> Items { get; set; } = [];
    public decimal TotalPrice => Items.Sum(item => item.Price * item.Quantity);

    public ShoppingCartDto(string username)
    {
        Username = username;
    }

    public ShoppingCartDto()
    {
    }
}