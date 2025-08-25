using BackendForFrontend.Api.Order.Contracts;
using BackendForFrontend.Api.Order.Dtos;

namespace BackendForFrontend.Api.Order.UseCases;

public class OrderUseCase(ILogger<OrderUseCase> logger, IOrderService service) : IOrderUseCase
{
    public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        logger.LogInformation("Creating order for customer: {CustomerId}", request.CustomerId);
        return await service.CreateOrderAsync(request);
    }

    public async Task<GetOrderResponse> GetOrderByIdAsync(Guid orderId)
    {
        logger.LogInformation("Retrieving order with ID: {OrderId}", orderId);
        return await service.GetOrderByIdAsync(orderId);
    }

    public async Task<UpdateOrderResponse> UpdateOrderAsync(UpdateOrderRequest request)
    {
        logger.LogInformation("Updating order with ID: {OrderId}", request.Id);
        return await service.UpdateOrderAsync(request);
    }

    public async Task<DeleteOrderResponse> DeleteOrderAsync(Guid orderId)
    {
        logger.LogInformation("Deleting order with ID: {OrderId}", orderId);
        return await service.DeleteOrderAsync(orderId);
    }

    public async Task<GetOrderListResponse> GetOrdersByCustomerIdAsync(Guid customerId)
    {
        logger.LogInformation("Retrieving orders for customer: {CustomerId}", customerId);
        return await service.GetOrdersByCustomerIdAsync(customerId);
    }
}
