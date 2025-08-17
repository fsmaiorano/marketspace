using BuildingBlocks;

namespace Basket.Api.Application.Basket.GetBasketById;

public interface IGetBasketByIdHandler
{
    Task<Result<GetBasketByIdResult>> HandleAsync(GetBasketByIdQuery query);
}