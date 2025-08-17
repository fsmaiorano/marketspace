namespace Basket.Api.Application.Basket.DeleteBasket;

public class DeleteBasketResult(bool isSuccess)
{
    public bool IsSuccess { get; init; } = isSuccess;
}