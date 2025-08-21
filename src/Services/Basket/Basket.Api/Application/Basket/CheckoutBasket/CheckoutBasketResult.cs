namespace Basket.Api.Application.Basket.CheckoutBasket;

public class CheckoutBasketResult(bool isSuccess)
{
    public bool IsSuccess { get; init; } = isSuccess;
}