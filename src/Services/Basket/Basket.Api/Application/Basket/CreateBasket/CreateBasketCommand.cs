using Basket.Api.Application.Dto;

namespace Basket.Api.Application.Basket.CreateBasket;

public class CreateBasketCommand
{
    public string Username { get; set; } = null!;
    public List<ShoppingCartItemDto> Items { get; set; } = [];
    public decimal TotalPrice => Items.Sum(item => item.Price * item.Quantity);

    public CreateBasketCommand(string username)
    {
        Username = username;
    }

    public CreateBasketCommand()
    {
    }
}