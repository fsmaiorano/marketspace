using BackendForFrontend.Api.Order.Contracts;

namespace BackendForFrontend.Test.Mocks;

public class TestOrderService(
    HttpClient httpClient, 
    IAppLogger<TestOrderService> logger) : IOrderService
{
    public async Task<Result<CreateOrderResponse>> CreateOrderAsync(CreateOrderRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Mock: Creating order for customer: {CustomerId}", request.CustomerId);

        Result<CreateOrderResponse> response = Result<CreateOrderResponse>.Success(new CreateOrderResponse
        {
            Id = Guid.CreateVersion7(),
            CustomerId = request.CustomerId,
            OrderName = $"Order-{DateTime.Now:yyyyMMddHHmmss}",
            ShippingAddress = request.ShippingAddress,
            BillingAddress = request.BillingAddress,
            Payment = request.Payment,
            Status = 1, // Created
            Items = request.Items,
            TotalAmount = request.Items.Sum(i => i.Price * i.Quantity)
        });

        return response;
    }

    public async Task<Result<GetOrderResponse>> GetOrderByIdAsync(Guid orderId)
    {
        logger.LogInformation(LogTypeEnum.Application, "Mock: Retrieving order with ID: {OrderId}", orderId);

        Result<GetOrderResponse> response = Result<GetOrderResponse>.Success(new GetOrderResponse
        {
            Id = orderId,
            CustomerId = Guid.CreateVersion7(),
            OrderName = $"Order-{DateTime.Now:yyyyMMddHHmmss}",
            ShippingAddress = new AddressDto { FirstName = "Test", LastName = "User.Api" },
            BillingAddress = new AddressDto { FirstName = "Test", LastName = "User.Api" },
            Payment = new PaymentDto { CardName = "Test Card" },
            Status = 1,
            Items = new List<OrderItemDto>(),
            TotalAmount = 100.00m
        });

        return response;
    }

    public async Task<Result<UpdateOrderResponse>> UpdateOrderAsync(UpdateOrderRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Mock: Updating order with ID: {OrderId}", request.Id);

        Result<UpdateOrderResponse> response = Result<UpdateOrderResponse>.Success(new UpdateOrderResponse
        {
            Id = request.Id, Status = request.Status
        });

        return response;
    }

    public async Task<Result<DeleteOrderResponse>> DeleteOrderAsync(Guid orderId)
    {
        logger.LogInformation(LogTypeEnum.Application, "Mock: Deleting order with ID: {OrderId}", orderId);

        Result<DeleteOrderResponse> response =
            Result<DeleteOrderResponse>.Success(new DeleteOrderResponse { IsDeleted = true });

        return response;
    }

    public async Task<Result<GetOrderListResponse>> GetOrdersByCustomerIdAsync(Guid customerId)
    {
        logger.LogInformation(LogTypeEnum.Application, "Mock: Retrieving orders for customer: {CustomerId}", customerId);

        Result<GetOrderListResponse> response = Result<GetOrderListResponse>.Success(new GetOrderListResponse
        {
            Orders =
            [
                new OrderSummaryDto
                {
                    Id = Guid.CreateVersion7(),
                    CustomerId = customerId,
                    OrderName = "Test Order 1",
                    Status = 1,
                    TotalAmount = 100.00m,
                    CreatedAt = DateTime.Now.AddDays(-1)
                },

                new OrderSummaryDto
                {
                    Id = Guid.CreateVersion7(),
                    CustomerId = customerId,
                    OrderName = "Test Order 2",
                    Status = 2,
                    TotalAmount = 200.00m,
                    CreatedAt = DateTime.Now.AddDays(-2)
                }
            ]
        });

        return response;
    }
}