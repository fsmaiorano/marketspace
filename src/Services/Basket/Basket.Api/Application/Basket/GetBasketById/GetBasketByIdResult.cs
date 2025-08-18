using Basket.Api.Application.Dto;

namespace Basket.Api.Application.Basket.GetBasketById;

public class GetBasketByIdResult
{
    public ShoppingCartDto ShoppingCart { get; init; }
    
    public GetBasketByIdResult(ShoppingCartDto shoppingCart)
    {
        ShoppingCart = shoppingCart;
    }
}