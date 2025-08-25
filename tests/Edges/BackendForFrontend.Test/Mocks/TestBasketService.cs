using BackendForFrontend.Api.Basket.Contracts;
using BackendForFrontend.Api.Basket.Dtos;
using Microsoft.Extensions.Logging;

namespace BackendForFrontend.Test.Mocks;

public class TestBasketService(HttpClient httpClient, ILogger<TestBasketService> logger) : IBasketService
{
    public Task<CreateBasketResponse> CreateBasketAsync(CreateBasketRequest request)
    {
        logger.LogInformation("Mock: Creating basket for user: {Username}", request.Username);
        
        CreateBasketResponse response = new CreateBasketResponse
        {
            Username = request.Username,
            Items = request.Items
        };

        return Task.FromResult(response);
    }

    public Task<GetBasketResponse> GetBasketByIdAsync(string username)
    {
        logger.LogInformation("Mock: Retrieving basket for user: {Username}", username);
        
        GetBasketResponse response = new GetBasketResponse
        {
            Username = username,
            Items =
            [
                new BasketItemDto
                {
                    ProductId = Guid.NewGuid().ToString(),
                    ProductName = "Test Product 1",
                    Quantity = 2,
                    Price = 25.00m
                },

                new BasketItemDto
                {
                    ProductId = Guid.NewGuid().ToString(),
                    ProductName = "Test Product 2",
                    Quantity = 1,
                    Price = 50.00m
                }
            ]
        };

        return Task.FromResult(response);
    }

    public Task<DeleteBasketResponse> DeleteBasketAsync(string username)
    {
        logger.LogInformation("Mock: Deleting basket for user: {Username}", username);
        
        DeleteBasketResponse response = new DeleteBasketResponse
        {
            IsDeleted = true
        };

        return Task.FromResult(response);
    }

    public Task<CheckoutBasketResponse> CheckoutBasketAsync(CheckoutBasketRequest request)
    {
        logger.LogInformation("Mock: Checking out basket for user: {Username}", request.Username);
        
        CheckoutBasketResponse response = new CheckoutBasketResponse
        {
            IsSuccess = true,
            Message = "Basket checked out successfully"
        };

        return Task.FromResult(response);
    }
}
