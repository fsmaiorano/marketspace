using BuildingBlocks;

namespace Order.Api.Application.Order.UpdateOrder;

public interface IUpdateOrderHandler
{
    Task<Result<UpdateOrderResult>> HandleAsync(UpdateOrderCommand command);
}