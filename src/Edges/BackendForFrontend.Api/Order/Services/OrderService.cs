using BackendForFrontend.Api.Base;
using BackendForFrontend.Api.Order.Contracts;
using BackendForFrontend.Api.Order.Dtos;
using BuildingBlocks;
using BuildingBlocks.Loggers;

namespace BackendForFrontend.Api.Order.Services;

public class OrderService(
    IAppLogger<OrderService> logger,
    HttpClient httpClient, 
    IConfiguration configuration)
    : BaseService(httpClient), IOrderService
{
    private string BaseUrl => configuration["Services:OrderService:BaseUrl"] ??
                              throw new ArgumentNullException($"OrderService BaseUrl is not configured");

    public async Task<Result<CreateOrderResponse>> CreateOrderAsync(CreateOrderRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Creating order for customer: {CustomerId}", request.CustomerId);
        
        HttpResponseMessage response = await DoPost($"{BaseUrl}/order", request);
        CreateOrderResponse? content = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation(LogTypeEnum.Business, "Order created successfully: {@Order}", content);
            return Result<CreateOrderResponse>.Success(content);
        }
        else
        {
            logger.LogError(LogTypeEnum.Application, null, "Failed to create order. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error creating order: {errorMessage}");
        }
    }

    public async Task<Result<GetOrderResponse>> GetOrderByIdAsync(Guid orderId)
    {
        logger.LogInformation(LogTypeEnum.Application, "Retrieving order with ID: {OrderId}", orderId);
        
        HttpResponseMessage response = await DoGet($"{BaseUrl}/order/{orderId}");
        Result<GetOrderResponse>? content = await response.Content.ReadFromJsonAsync<Result<GetOrderResponse>>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation(LogTypeEnum.Application, "Order retrieved successfully: {@Order}", content);
            return content;
        }
        else
        {
            logger.LogError(LogTypeEnum.Application, null, "Failed to retrieve order. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error retrieving order: {errorMessage}");
        }
    }

    public async Task<Result<UpdateOrderResponse>> UpdateOrderAsync(UpdateOrderRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Updating order with ID: {OrderId}", request.Id);
        
        HttpResponseMessage response = await DoPut($"{BaseUrl}/order", request);
        Result<UpdateOrderResponse>? content = await response.Content.ReadFromJsonAsync<Result<UpdateOrderResponse>>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation(LogTypeEnum.Business, "Order updated successfully: {@Order}", content);
            return content;
        }
        else
        {
            logger.LogError(LogTypeEnum.Application, null, "Failed to update order. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error updating order: {errorMessage}");
        }
    }

    public async Task<Result<DeleteOrderResponse>> DeleteOrderAsync(Guid orderId)
    {
        logger.LogInformation(LogTypeEnum.Application, "Deleting order with ID: {OrderId}", orderId);
        
        HttpResponseMessage response = await DoDelete($"{BaseUrl}/order/{orderId}");
        Result<DeleteOrderResponse>? content = await response.Content.ReadFromJsonAsync<Result<DeleteOrderResponse>>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation(LogTypeEnum.Business, "Order deleted successfully: {@Order}", content);
            return content;
        }
        else
        {
            logger.LogError(LogTypeEnum.Application, null, "Failed to delete order. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error deleting order: {errorMessage}");
        }
    }

    public async Task<Result<GetOrderListResponse>> GetOrdersByCustomerIdAsync(Guid customerId)
    {
        logger.LogInformation(LogTypeEnum.Application, "Retrieving orders for customer: {CustomerId}", customerId);
        
        HttpResponseMessage response = await DoGet($"{BaseUrl}/order/customer/{customerId}");
        Result<GetOrderListResponse>? content = await response.Content.ReadFromJsonAsync<Result<GetOrderListResponse>>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation(LogTypeEnum.Application, "Orders retrieved successfully for customer: {CustomerId} with {Count} orders", 
                customerId, content.Data?.Orders.Count);
            return content;
        }
        else
        {
            logger.LogError(LogTypeEnum.Application, null, "Failed to retrieve orders. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error retrieving orders: {errorMessage}");
        }
    }
}
