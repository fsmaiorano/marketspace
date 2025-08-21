using BuildingBlocks;

namespace Basket.Api.Application.Basket.CheckoutBasket;

public class CheckoutBasketHandler : ICheckoutBasketHandler
{
    // Start integration between Basket and Order services
    public Task<Result<CheckoutBasketResult>> HandleAsync(CheckoutBasketCommand command)
    {
        throw new NotImplementedException();
    }
}