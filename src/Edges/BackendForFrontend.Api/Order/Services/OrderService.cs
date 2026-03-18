using BackendForFrontend.Api.Base;
using BackendForFrontend.Api.Order.Dtos;
using BuildingBlocks;
using BuildingBlocks.Loggers;

namespace BackendForFrontend.Api.Order.Services;

public class OrderService(
    IAppLogger<OrderService> logger,
    HttpClient httpClient,
    IConfiguration configuration)
    : BaseService(httpClient)
{
    private string BaseUrl => configuration["Services:OrderService:BaseUrl"] ??
                              throw new ArgumentNullException($"OrderService BaseUrl is not configured");

    private sealed class MsOrderListResponse
    {
        public List<MsOrderDto> Orders { get; set; } = [];
    }

    private sealed record MsOrderItemDto(Guid OrderId, Guid CatalogId, int Quantity, decimal Price);
    private sealed record MsOrderDto(Guid Id, Guid CustomerId, AddressDto? ShippingAddress, AddressDto? BillingAddress,
        PaymentDto? Payment, string? Status, List<MsOrderItemDto>? Items, decimal TotalAmount, DateTimeOffset CreatedAt);

    public async Task<Result<CreateOrderResponse>> CreateOrderAsync(CreateOrderRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Creating order for customer: {CustomerId}", request.CustomerId);

        HttpResponseMessage response = await DoPost($"{BaseUrl}/order", request);

        if (response.StatusCode == System.Net.HttpStatusCode.Created)
        {
            logger.LogInformation(LogTypeEnum.Business, "Order created successfully");
            return Result<CreateOrderResponse>.Success(new CreateOrderResponse());
        }

        logger.LogError(LogTypeEnum.Application, null, "Failed to create order. Status code: {StatusCode}", response.StatusCode);
        string errorMessage = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Error creating order: {errorMessage}");
    }

    public async Task<Result<GetOrderResponse>> GetOrderByIdAsync(Guid orderId)
    {
        logger.LogInformation(LogTypeEnum.Application, "Retrieving order with ID: {OrderId}", orderId);

        HttpResponseMessage response = await DoGet($"{BaseUrl}/order/{orderId}");

        if (response.IsSuccessStatusCode)
        {
            MsOrderDto? order = await response.Content.ReadFromJsonAsync<MsOrderDto>(
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (order is not null)
            {
                GetOrderResponse mapped = new()
                {
                    Id = order.Id,
                    CustomerId = order.CustomerId,
                    ShippingAddress = order.ShippingAddress ?? new(),
                    BillingAddress = order.BillingAddress ?? new(),
                    Payment = order.Payment ?? new(),
                    Status = order.Status ?? string.Empty,
                    Items = order.Items?.Select(i => new OrderItemDto
                    {
                        ProductId = i.CatalogId,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList() ?? [],
                    TotalAmount = order.TotalAmount,
                    CreatedAt = order.CreatedAt
                };

                logger.LogInformation(LogTypeEnum.Application, "Order retrieved successfully: {OrderId}", order.Id);
                return Result<GetOrderResponse>.Success(mapped);
            }
            return Result<GetOrderResponse>.Failure("Order not found");
        }

        logger.LogError(LogTypeEnum.Application, null, "Failed to retrieve order. Status code: {StatusCode}", response.StatusCode);
        string errorMessage = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Error retrieving order: {errorMessage}");
    }

    public async Task<Result<UpdateOrderResponse>> UpdateOrderAsync(UpdateOrderRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Updating order with ID: {OrderId}", request.Id);

        HttpResponseMessage response = await DoPut($"{BaseUrl}/order", request);

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            logger.LogInformation(LogTypeEnum.Business, "Order updated successfully");
            return Result<UpdateOrderResponse>.Success(new UpdateOrderResponse());
        }

        logger.LogError(LogTypeEnum.Application, null, "Failed to update order. Status code: {StatusCode}", response.StatusCode);
        string errorMessage = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Error updating order: {errorMessage}");
    }

    public async Task<Result<DeleteOrderResponse>> DeleteOrderAsync(Guid orderId)
    {
        logger.LogInformation(LogTypeEnum.Application, "Deleting order with ID: {OrderId}", orderId);

        HttpResponseMessage response = await DoDelete($"{BaseUrl}/order/{orderId}");

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            logger.LogInformation(LogTypeEnum.Business, "Order deleted successfully");
            return Result<DeleteOrderResponse>.Success(new DeleteOrderResponse { IsDeleted = true });
        }

        logger.LogError(LogTypeEnum.Application, null, "Failed to delete order. Status code: {StatusCode}", response.StatusCode);
        string errorMessage = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Error deleting order: {errorMessage}");
    }

    public async Task<Result<GetOrderListResponse>> GetOrdersByCustomerIdAsync(Guid customerId)
    {
        logger.LogInformation(LogTypeEnum.Application, "Retrieving orders for customer: {CustomerId}", customerId);

        HttpResponseMessage response = await DoGet($"{BaseUrl}/order/customer/{customerId}");

        if (response.IsSuccessStatusCode)
        {
            List<MsOrderDto>? orders = await response.Content.ReadFromJsonAsync<List<MsOrderDto>>(
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            GetOrderListResponse result = new()
            {
                Orders = (orders ?? []).Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    OrderName = $"Order #{o.Id.ToString().ToUpper()}",
                    Status = MapOrderStatus(o.Status),
                    TotalAmount = o.TotalAmount,
                    CreatedAt = o.CreatedAt.UtcDateTime
                }).ToList()
            };

            logger.LogInformation(LogTypeEnum.Application,
                "Orders retrieved successfully for customer: {CustomerId} with {Count} orders",
                customerId, result.Orders.Count);

            return Result<GetOrderListResponse>.Success(result);
        }

        logger.LogError(LogTypeEnum.Application, null, "Failed to retrieve orders. Status code: {StatusCode}", response.StatusCode);
        string errorMessage = await response.Content.ReadAsStringAsync();
        return Result<GetOrderListResponse>.Failure($"Error retrieving orders: {errorMessage}");
    }

    private static int MapOrderStatus(string? status) => status switch
    {
        "Created" => 1,
        "Processing" => 2,
        "Completed" => 3,
        "ReadyForDelivery" => 4,
        "DeliveryInProgress" => 5,
        "Delivered" => 6,
        "Finalized" => 7,
        "Cancelled" => 90,
        "CancelledByCustomer" => 91,
        _ => 0
    };

    public async Task<Result<GetOrdersByCatalogIdsResponse>> GetOrdersByCatalogIdsAsync(IEnumerable<Guid> catalogIds, int limit = 50)
    {
        List<Guid> ids = catalogIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return Result<GetOrdersByCatalogIdsResponse>.Success(new GetOrdersByCatalogIdsResponse());

        HttpResponseMessage response = await DoPost($"{BaseUrl}/order/catalog-items", new
        {
            catalogIds = ids,
            limit
        });

        if (response.IsSuccessStatusCode)
        {
            MsOrderListResponse? content = await response.Content.ReadFromJsonAsync<MsOrderListResponse>(
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (content is null)
                return Result<GetOrdersByCatalogIdsResponse>.Success(new GetOrdersByCatalogIdsResponse());

            GetOrdersByCatalogIdsResponse mapped = new()
            {
                Orders = content.Orders.Select(order => new GetOrderResponse
                {
                    Id = order.Id,
                    CustomerId = order.CustomerId,
                    ShippingAddress = order.ShippingAddress ?? new AddressDto(),
                    BillingAddress = order.BillingAddress ?? new AddressDto(),
                    Payment = order.Payment ?? new PaymentDto(),
                    Status = order.Status ?? string.Empty,
                    Items = order.Items?.Select(item => new OrderItemDto
                    {
                        ProductId = item.CatalogId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    }).ToList() ?? [],
                    TotalAmount = order.TotalAmount,
                    CreatedAt = order.CreatedAt
                }).ToList()
            };

            return Result<GetOrdersByCatalogIdsResponse>.Success(mapped);
        }

        string errorMessage = await response.Content.ReadAsStringAsync();
        return Result<GetOrdersByCatalogIdsResponse>.Failure($"Failed to retrieve merchant orders: {errorMessage}");
    }
}
