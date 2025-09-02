namespace WebApp.Dtos;

public class GetBasketResponse
{
    public ShoppingCartDto Cart { get; set; } = new();
}