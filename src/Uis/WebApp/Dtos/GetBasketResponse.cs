namespace WebApp.Dtos;

public class GetBasketResponse
{
    public string Username { get; set; } = string.Empty;
    public List<ShoppingCartItemDto> Items { get; set; } = [];
}