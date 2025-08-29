namespace WebApp.Dtos;

public class CreateOrUpdateBasketResponse
{
    public string Username { get; init; }
    public List<ShoppingCartItemDto> Items { get; init; } = [];
}