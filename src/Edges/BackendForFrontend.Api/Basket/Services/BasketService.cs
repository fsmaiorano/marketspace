using BackendForFrontend.Api.Base;
using BackendForFrontend.Api.Basket.Dtos;
using BuildingBlocks;
using BuildingBlocks.Loggers;

namespace BackendForFrontend.Api.Basket.Services;

public class BasketService(
    IAppLogger<BasketService> logger,
    HttpClient httpClient,
    IConfiguration configuration)
    : BaseService(httpClient)
{
    private string BaseUrl => configuration["Services:BasketService:BaseUrl"] ??
                              throw new ArgumentNullException($"BasketService BaseUrl is not configured");

    public async Task<Result<CreateBasketResponse>> CreateBasketAsync(CreateBasketRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Creating basket for user: {Username}", request.Username);

        HttpResponseMessage response = await DoPost($"{BaseUrl}/basket", request);

        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation(LogTypeEnum.Business, "Basket created successfully for user: {Username}, fetching basket data", request.Username);
            
            Result<GetBasketResponse> getResult = await GetBasketByIdAsync(request.Username);
            
            if (getResult.IsSuccess && getResult.Data is not null)
            {
                return Result<CreateBasketResponse>.Success(new CreateBasketResponse
                {
                    ShoppingCart = getResult.Data.ShoppingCart
                });
            }
            
            return Result<CreateBasketResponse>.Failure(getResult.Error ?? "Failed to retrieve created basket");
        }
        else
        {
            logger.LogError(LogTypeEnum.Application, null, "Failed to create basket. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error creating basket: {errorMessage}");
        }
    }

    public async Task<Result<GetBasketResponse>> GetBasketByIdAsync(string username)
    {
        logger.LogInformation(LogTypeEnum.Application, "Retrieving basket for user: {Username}", username);

        HttpResponseMessage response = await DoGet($"{BaseUrl}/basket/{username}");

        if (response.IsSuccessStatusCode)
        {
            CartDto? cart = await response.Content.ReadFromJsonAsync<CartDto>();

            if (cart is not null)
            {
                logger.LogInformation(LogTypeEnum.Application, "Basket retrieved successfully for user: {Username}", username);
                return Result<GetBasketResponse>.Success(new GetBasketResponse
                {
                    ShoppingCart = cart
                });
            }

            logger.LogError(LogTypeEnum.Application, null, "Basket response was null for user: {Username}", username);
            return Result<GetBasketResponse>.Failure("Basket not found");
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.NotFound || 
                 response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            logger.LogInformation(LogTypeEnum.Application, "Basket not found for user: {Username}", username);
            return Result<GetBasketResponse>.Success(new GetBasketResponse
            {
                ShoppingCart = new CartDto { Username = username, Items = [] }
            });
        }
        else
        {
            logger.LogError(LogTypeEnum.Application, null, "Failed to retrieve basket. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error retrieving basket: {errorMessage}");
        }
    }

    public async Task<Result<DeleteBasketResponse>> DeleteBasketAsync(string username)
    {
        logger.LogInformation(LogTypeEnum.Application, "Deleting basket for user: {Username}", username);

        try
        {
            HttpResponseMessage response = await DoDelete($"{BaseUrl}/basket/{username}");

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                logger.LogInformation(LogTypeEnum.Business, "Basket deleted successfully for user: {Username}", username);
                return Result<DeleteBasketResponse>.Success(new DeleteBasketResponse { IsSuccess = true });
            }
            else if (response.IsSuccessStatusCode)
            {
                logger.LogInformation(LogTypeEnum.Business, "Basket deleted successfully for user: {Username}", username);
                return Result<DeleteBasketResponse>.Success(new DeleteBasketResponse { IsSuccess = true });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound || 
                     response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                logger.LogInformation(LogTypeEnum.Application, "Basket not found for user: {Username}, treating as success", username);
                return Result<DeleteBasketResponse>.Success(new DeleteBasketResponse { IsSuccess = true });
            }
            else
            {
                logger.LogError(LogTypeEnum.Application, null, "Failed to delete basket. Status code: {StatusCode}",
                    response.StatusCode);
                return Result<DeleteBasketResponse>.Success(new DeleteBasketResponse { IsSuccess = true });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "Exception occurred while deleting basket for user: {Username}", username);
            return Result<DeleteBasketResponse>.Success(new DeleteBasketResponse { IsSuccess = true });
        }
    }

    public async Task<Result<CheckoutBasketResponse>> CheckoutBasketAsync(CheckoutBasketRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Checking out basket for user: {Username}", request.Username);

        HttpResponseMessage response = await DoPost($"{BaseUrl}/basket/checkout", request);
        Result<CheckoutBasketResponse>? content = await response.Content.ReadFromJsonAsync<Result<CheckoutBasketResponse>>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation(LogTypeEnum.Business, "Basket checkout completed successfully for user: {Username}", request.Username);
            return content;
        }
        else
        {
            logger.LogError(LogTypeEnum.Application, null, "Failed to checkout basket. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error checking out basket: {errorMessage}");
        }
    }
}
