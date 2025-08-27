using BackendForFrontend.Api.Order.Dtos;
using BuildingBlocks;

namespace BackendForFrontend.Api.Order.Contracts;

public interface IOrderService
{
    Task<Result<CreateOrderResponse>> CreateOrderAsync(CreateOrderRequest request);
    Task<Result<GetOrderResponse>> GetOrderByIdAsync(Guid orderId);
    Task<Result<UpdateOrderResponse>> UpdateOrderAsync(UpdateOrderRequest request);
    Task<Result<DeleteOrderResponse>> DeleteOrderAsync(Guid orderId);
    Task<Result<GetOrderListResponse>> GetOrdersByCustomerIdAsync(Guid customerId);
}
