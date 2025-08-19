using BuildingBlocks;

namespace Order.Api.Application.Order.DeleteOrder;

public interface IDeleteOrderHandler
{
    Task<Result<DeleteOrderResult>> HandleAsync(DeleteOrderCommand command);
}