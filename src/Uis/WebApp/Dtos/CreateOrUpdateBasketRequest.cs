namespace WebApp.Dtos;

public class CreateOrUpdateBasketRequest
{
    public string Username { get; set; } = null!;
    public List<ShoppingCartItemDto> Items { get; set; } = [];
}