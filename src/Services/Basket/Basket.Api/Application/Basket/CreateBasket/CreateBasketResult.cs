using Basket.Api.Application.Dto;

namespace Basket.Api.Application.Basket.CreateBasket;

public class CreateBasketResult(ShoppingCartDto cart)
{
    public ShoppingCartDto ShoppingCart { get; init; } = cart;
}