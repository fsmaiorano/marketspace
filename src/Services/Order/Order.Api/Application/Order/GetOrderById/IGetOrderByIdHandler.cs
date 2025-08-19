using BuildingBlocks;

namespace Order.Api.Application.Order.GetOrderById;

public interface IGetOrderByIdHandler
{
    Task<Result<GetOrderByIdResult>> HandleAsync(GetOrderByIdQuery query);
}