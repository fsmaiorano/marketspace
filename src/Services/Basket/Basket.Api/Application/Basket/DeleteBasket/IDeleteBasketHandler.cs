using BuildingBlocks;

namespace Basket.Api.Application.Basket.DeleteBasket;

public interface IDeleteBasketHandler
{
    Task<Result<DeleteBasketResult>> HandleAsync(DeleteBasketCommand command);
}