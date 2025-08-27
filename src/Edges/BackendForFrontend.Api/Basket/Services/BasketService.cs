using BackendForFrontend.Api.Base;
using BackendForFrontend.Api.Basket.Contracts;
using BackendForFrontend.Api.Basket.Dtos;
using BuildingBlocks;

namespace BackendForFrontend.Api.Basket.Services;

public class BasketService(ILogger<BasketService> logger, HttpClient httpClient, IConfiguration configuration)
    : BaseService(httpClient), IBasketService
{
    private string BaseUrl => configuration["Services:BasketService:BaseUrl"] ??
                              throw new ArgumentNullException($"BasketService BaseUrl is not configured");

    public async Task<Result<CreateBasketResponse>> CreateBasketAsync(CreateBasketRequest request)
    {
        logger.LogInformation("Creating basket for user: {Username}", request.Username);
        
        HttpResponseMessage response = await DoPost($"{BaseUrl}/basket", request);
        Result<CreateBasketResponse>? content = await response.Content.ReadFromJsonAsync<Result<CreateBasketResponse>>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation("Basket created successfully for user: {Username}", request.Username);
            return content;
        }
        else
        {
            logger.LogError("Failed to create basket. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error creating basket: {errorMessage}");
        }
    }

    public async Task<Result<GetBasketResponse>> GetBasketByIdAsync(string username)
    {
        logger.LogInformation("Retrieving basket for user: {Username}", username);
        
        HttpResponseMessage response = await DoGet($"{BaseUrl}/basket/{username}");
        Result<GetBasketResponse>? content = await response.Content.ReadFromJsonAsync<Result<GetBasketResponse>>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation("Basket retrieved successfully for user: {Username}", username);
            return content;
        }
        else
        {
            logger.LogError("Failed to retrieve basket. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error retrieving basket: {errorMessage}");
        }
    }

    public async Task<Result<DeleteBasketResponse>> DeleteBasketAsync(string username)
    {
        logger.LogInformation("Deleting basket for user: {Username}", username);
        
        HttpResponseMessage response = await DoDelete($"{BaseUrl}/basket/{username}");
        Result<DeleteBasketResponse>? content = await response.Content.ReadFromJsonAsync<Result<DeleteBasketResponse>>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation("Basket deleted successfully for user: {Username}", username);
            return content;
        }
        else
        {
            logger.LogError("Failed to delete basket. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error deleting basket: {errorMessage}");
        }
    }

    public async Task<Result<CheckoutBasketResponse>> CheckoutBasketAsync(CheckoutBasketRequest request)
    {
        logger.LogInformation("Checking out basket for user: {Username}", request.Username);
        
        HttpResponseMessage response = await DoPost($"{BaseUrl}/basket/checkout", request);
        Result<CheckoutBasketResponse>? content = await response.Content.ReadFromJsonAsync<Result<CheckoutBasketResponse>>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation("Basket checkout completed successfully for user: {Username}", request.Username);
            return content;
        }
        else
        {
            logger.LogError("Failed to checkout basket. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error checking out basket: {errorMessage}");
        }
    }
}
