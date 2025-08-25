namespace BackendForFrontend.Api.Basket.Dtos;

public class CheckoutBasketResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}
