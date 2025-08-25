using BackendForFrontend.Api.Order.Contracts;
using BackendForFrontend.Api.Order.Dtos;
using Microsoft.Extensions.Logging;

namespace BackendForFrontend.Test.Mocks;

public class TestOrderService(HttpClient httpClient, ILogger<TestOrderService> logger) : IOrderService
{
    public Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        logger.LogInformation("Mock: Creating order for customer: {CustomerId}", request.CustomerId);
        
        var response = new CreateOrderResponse
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            OrderName = $"Order-{DateTime.Now:yyyyMMddHHmmss}",
            ShippingAddress = request.ShippingAddress,
            BillingAddress = request.BillingAddress,
            Payment = request.Payment,
            Status = 1, // Created
            Items = request.Items,
            TotalAmount = request.Items.Sum(i => i.Price * i.Quantity)
        };

        return Task.FromResult(response);
    }

    public Task<GetOrderResponse> GetOrderByIdAsync(Guid orderId)
    {
        logger.LogInformation("Mock: Retrieving order with ID: {OrderId}", orderId);
        
        var response = new GetOrderResponse
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            OrderName = $"Order-{DateTime.Now:yyyyMMddHHmmss}",
            ShippingAddress = new AddressDto { FirstName = "Test", LastName = "User" },
            BillingAddress = new AddressDto { FirstName = "Test", LastName = "User" },
            Payment = new PaymentDto { CardName = "Test Card" },
            Status = 1,
            Items = new List<OrderItemDto>(),
            TotalAmount = 100.00m
        };

        return Task.FromResult(response);
    }

    public Task<UpdateOrderResponse> UpdateOrderAsync(UpdateOrderRequest request)
    {
        logger.LogInformation("Mock: Updating order with ID: {OrderId}", request.Id);
        
        var response = new UpdateOrderResponse
        {
            Id = request.Id,
            CustomerId = request.CustomerId,
            OrderName = $"Updated-Order-{DateTime.Now:yyyyMMddHHmmss}",
            ShippingAddress = request.ShippingAddress,
            BillingAddress = request.BillingAddress,
            Payment = request.Payment,
            Status = request.Status,
            Items = request.Items,
            TotalAmount = request.TotalAmount
        };

        return Task.FromResult(response);
    }

    public Task<DeleteOrderResponse> DeleteOrderAsync(Guid orderId)
    {
        logger.LogInformation("Mock: Deleting order with ID: {OrderId}", orderId);
        
        var response = new DeleteOrderResponse
        {
            IsDeleted = true
        };

        return Task.FromResult(response);
    }

    public Task<GetOrderListResponse> GetOrdersByCustomerIdAsync(Guid customerId)
    {
        logger.LogInformation("Mock: Retrieving orders for customer: {CustomerId}", customerId);
        
        var response = new GetOrderListResponse
        {
            Orders = new List<OrderSummaryDto>
            {
                new OrderSummaryDto
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    OrderName = "Test Order 1",
                    Status = 1,
                    TotalAmount = 100.00m,
                    CreatedAt = DateTime.Now.AddDays(-1)
                },
                new OrderSummaryDto
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    OrderName = "Test Order 2",
                    Status = 2,
                    TotalAmount = 200.00m,
                    CreatedAt = DateTime.Now.AddDays(-2)
                }
            }
        };

        return Task.FromResult(response);
    }
}