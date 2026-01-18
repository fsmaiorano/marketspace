using BackendForFrontend.Api.Order.Contracts;
using BackendForFrontend.Api.Order.Dtos;
using BuildingBlocks;
using BuildingBlocks.Loggers;

namespace BackendForFrontend.Api.Order.UseCases;

public class OrderUseCase(
    IAppLogger<OrderUseCase> logger,
    IOrderService service) : IOrderUseCase
{
    public async Task<Result<CreateOrderResponse>> CreateOrderAsync(CreateOrderRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Creating order for customer: {CustomerId}", request.CustomerId);
        return await service.CreateOrderAsync(request);
    }

    public async Task<Result<GetOrderResponse>> GetOrderByIdAsync(Guid orderId)
    {
        logger.LogInformation(LogTypeEnum.Application, "Retrieving order with ID: {OrderId}", orderId);
        return await service.GetOrderByIdAsync(orderId);
    }

    public async Task<Result<UpdateOrderResponse>> UpdateOrderAsync(UpdateOrderRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Updating order with ID: {OrderId}", request.Id);
        return await service.UpdateOrderAsync(request);
    }

    public async Task<Result<DeleteOrderResponse>> DeleteOrderAsync(Guid orderId)
    {
        logger.LogInformation(LogTypeEnum.Application, "Deleting order with ID: {OrderId}", orderId);
        return await service.DeleteOrderAsync(orderId);
    }

    public async Task<Result<GetOrderListResponse>> GetOrdersByCustomerIdAsync(Guid customerId)
    {
        logger.LogInformation(LogTypeEnum.Application, "Retrieving orders for customer: {CustomerId}", customerId);
        return await service.GetOrdersByCustomerIdAsync(customerId);
    }
}
