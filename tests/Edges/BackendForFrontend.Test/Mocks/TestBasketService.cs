using BackendForFrontend.Api.Basket.Contracts;
using BackendForFrontend.Api.Basket.Dtos;
using Basket.Api.Application.Basket.CreateBasket;
using Basket.Api.Application.Basket.GetBasketById;
using BuildingBlocks;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace BackendForFrontend.Test.Mocks;

public class TestBasketService(HttpClient httpClient, ILogger<TestBasketService> logger) : IBasketService
{
    public async Task<CreateBasketResponse> CreateBasketAsync(CreateBasketRequest request)
    {
        try
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/basket", request);

            if (response.IsSuccessStatusCode)
            {
                Result<CreateBasketResult>? resultWrapper =
                    await response.Content.ReadFromJsonAsync<Result<CreateBasketResult>>();

                if (resultWrapper is { IsSuccess: true, Data: not null })
                {
                    CreateBasketResponse basketResponse = new CreateBasketResponse
                    {
                        Username = request.Username, Items = request.Items
                    };

                    logger.LogInformation("Basket created successfully: {@Basket}", basketResponse);
                    return basketResponse;
                }
            }

            logger.LogError("Failed to create basket. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error creating basket: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating basket");
            throw;
        }
    }

    public async Task<GetBasketResponse> GetBasketByIdAsync(string username)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync($"/basket/{username}");

            if (response.IsSuccessStatusCode)
            {
                Result<GetBasketByIdResult>? resultWrapper =
                    await response.Content.ReadFromJsonAsync<Result<GetBasketByIdResult>>();

                if (resultWrapper is { IsSuccess: true, Data: not null })
                {
                    GetBasketResponse basketResponse = new GetBasketResponse
                    {
                        Username = resultWrapper.Data.ShoppingCart.Username,
                        Items = resultWrapper.Data.ShoppingCart.Items.Select(item => new BasketItemDto
                        {
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            Quantity = item.Quantity,
                            Price = item.Price,
                        }).ToList()
                    };

                    logger.LogInformation("Basket retrieved successfully: {@Basket}", basketResponse);
                    return basketResponse;
                }
            }

            logger.LogError("Failed to retrieve basket. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error retrieving basket: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while retrieving basket");
            throw;
        }
    }

    public async Task<DeleteBasketResponse> DeleteBasketAsync(string username)
    {
        try
        {
            HttpResponseMessage response = await httpClient.DeleteAsync($"/basket/{username}");

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Basket deleted successfully for user: {Username}", username);
                return new DeleteBasketResponse { IsSuccess = true };
            }

            logger.LogError("Failed to delete basket. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error deleting basket: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting basket");
            throw;
        }
    }

    public async Task<CheckoutBasketResponse> CheckoutBasketAsync(CheckoutBasketRequest request)
    {
        try
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/basket/checkout", request);
            
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Basket checkout initiated successfully for user: {Username}", request.Username);
                return new CheckoutBasketResponse { IsSuccess = true };
            }
            
            logger.LogError("Failed to checkout basket. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error checking out basket: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while checking out basket");
            throw;
        }
    }
}