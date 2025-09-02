namespace WebApp.Dtos;

public class ShoppingCartDto
{
    public string Username { get; set; }
    public List<ShoppingCartItemDto> Items { get; set; }
    public decimal TotalPrice => Items.Sum(item => item.Price * item.Quantity);
}