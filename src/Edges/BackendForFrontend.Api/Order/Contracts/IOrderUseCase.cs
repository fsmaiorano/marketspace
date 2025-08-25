using BackendForFrontend.Api.Order.Dtos;

namespace BackendForFrontend.Api.Order.Contracts;

public interface IOrderUseCase
{
    Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<GetOrderResponse> GetOrderByIdAsync(Guid orderId);
    Task<UpdateOrderResponse> UpdateOrderAsync(UpdateOrderRequest request);
    Task<DeleteOrderResponse> DeleteOrderAsync(Guid orderId);
    Task<GetOrderListResponse> GetOrdersByCustomerIdAsync(Guid customerId);
}
