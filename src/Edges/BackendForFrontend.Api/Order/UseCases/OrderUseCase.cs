using BackendForFrontend.Api.Order.Contracts;
using BackendForFrontend.Api.Order.Dtos;
using BuildingBlocks;
using BuildingBlocks.Loggers.Abstractions;

namespace BackendForFrontend.Api.Order.UseCases;

public class OrderUseCase(
    IApplicationLogger<OrderUseCase> applicationLogger,
    IBusinessLogger<OrderUseCase> businessLogger,
    IOrderService service) : IOrderUseCase
{
    public async Task<Result<CreateOrderResponse>> CreateOrderAsync(CreateOrderRequest request)
    {
        applicationLogger.LogInformation("Creating order for customer: {CustomerId}", request.CustomerId);
        return await service.CreateOrderAsync(request);
    }

    public async Task<Result<GetOrderResponse>> GetOrderByIdAsync(Guid orderId)
    {
        applicationLogger.LogInformation("Retrieving order with ID: {OrderId}", orderId);
        return await service.GetOrderByIdAsync(orderId);
    }

    public async Task<Result<UpdateOrderResponse>> UpdateOrderAsync(UpdateOrderRequest request)
    {
        applicationLogger.LogInformation("Updating order with ID: {OrderId}", request.Id);
        return await service.UpdateOrderAsync(request);
    }

    public async Task<Result<DeleteOrderResponse>> DeleteOrderAsync(Guid orderId)
    {
        applicationLogger.LogInformation("Deleting order with ID: {OrderId}", orderId);
        return await service.DeleteOrderAsync(orderId);
    }

    public async Task<Result<GetOrderListResponse>> GetOrdersByCustomerIdAsync(Guid customerId)
    {
        applicationLogger.LogInformation("Retrieving orders for customer: {CustomerId}", customerId);
        return await service.GetOrdersByCustomerIdAsync(customerId);
    }
}
