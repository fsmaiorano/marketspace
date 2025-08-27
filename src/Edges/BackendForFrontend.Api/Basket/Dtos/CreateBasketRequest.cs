namespace BackendForFrontend.Api.Basket.Dtos;

public class CreateBasketRequest
{
    public string Username { get; set; } = string.Empty;
    public List<BasketItemDto> Items { get; set; } = new();
}