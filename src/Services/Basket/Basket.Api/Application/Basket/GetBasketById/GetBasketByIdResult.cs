using Basket.Api.Application.Dto;

namespace Basket.Api.Application.Basket.GetBasketById;

public class GetBasketByIdResult(ShoppingCartDto shoppingCart)
{
    public ShoppingCartDto ShoppingCart { get; init; } = shoppingCart;
}