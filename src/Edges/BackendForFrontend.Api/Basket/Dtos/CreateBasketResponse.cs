namespace BackendForFrontend.Api.Basket.Dtos;

public class CreateBasketResponse
{
    public CartDto ShoppingCart { get; set; } = new();
}