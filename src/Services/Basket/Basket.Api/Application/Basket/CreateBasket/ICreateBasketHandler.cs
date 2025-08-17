using BuildingBlocks;

namespace Basket.Api.Application.Basket.CreateBasket;

public interface ICreateBasketHandler
{
    Task<Result<CreateBasketResult>> HandleAsync(CreateBasketCommand command);
}