namespace BackendForFrontend.Api.Basket.Dtos;

public class CartDto
{
    public string? Username { get; init; } = null;
    public List<BasketItemDto> Items { get; init; } = [];
    public decimal TotalPrice => Items.Sum(item => item.Price * item.Quantity);
}