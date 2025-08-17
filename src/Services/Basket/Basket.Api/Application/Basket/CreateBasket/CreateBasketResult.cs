using Basket.Api.Application.Dto;

namespace Basket.Api.Application.Basket.CreateBasket;

public class CreateBasketResult
{
    public ShoppingCartDto ShoppingCart { get; set; }

    public CreateBasketResult() { }

    public CreateBasketResult(ShoppingCartDto shoppingCart)
    {
        ShoppingCart = shoppingCart;
    }
}