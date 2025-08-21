using BuildingBlocks;

namespace Basket.Api.Application.Basket.CheckoutBasket;

public interface ICheckoutBasketHandler
{
    Task<Result<CheckoutBasketResult>> HandleAsync(CheckoutBasketCommand command);   
}