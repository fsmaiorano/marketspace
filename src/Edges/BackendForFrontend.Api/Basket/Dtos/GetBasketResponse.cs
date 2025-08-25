namespace BackendForFrontend.Api.Basket.Dtos;

public class GetBasketResponse
{
    public string Username { get; set; } = string.Empty;
    public List<BasketItemDto> Items { get; set; } = new();
}
