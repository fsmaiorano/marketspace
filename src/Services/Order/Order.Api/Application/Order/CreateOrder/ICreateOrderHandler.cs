using BuildingBlocks;

namespace Order.Api.Application.Order.CreateOrder;

public interface ICreateOrderHandler
{
    Task<Result<CreateOrderResult>> HandleAsync(CreateOrderCommand command);
}