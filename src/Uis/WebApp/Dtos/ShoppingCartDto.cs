namespace WebApp.Dtos;

public class ShoppingCartDto
{
    public string Username { get; set; } = string.Empty;
    public List<ShoppingCartItemDto> Items { get; set; } = [];
}