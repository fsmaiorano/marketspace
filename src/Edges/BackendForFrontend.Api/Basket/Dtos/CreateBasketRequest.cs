namespace BackendForFrontend.Api.Basket.Dtos;

public class CreateBasketRequest
{
    public string Username { get; set; } = string.Empty;
    public List<BasketItemDto> Items { get; set; } = new();
}

public class BasketItemDto
{
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
}
