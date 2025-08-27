using BackendForFrontend.Api.Basket.Contracts;
using BackendForFrontend.Api.Basket.Dtos;
using BuildingBlocks;

namespace BackendForFrontend.Api.Basket.UseCases;

public class BasketUseCase(ILogger<BasketUseCase> logger, IBasketService service) : IBasketUseCase
{
    public async Task<Result<CreateBasketResponse>> CreateBasketAsync(CreateBasketRequest request)
    {
        logger.LogInformation("Creating basket for user: {Username}", request.Username);
        return await service.CreateBasketAsync(request);
    }

    public async Task<Result<GetBasketResponse>> GetBasketByIdAsync(string username)
    {
        logger.LogInformation("Retrieving basket for user: {Username}", username);
        return await service.GetBasketByIdAsync(username);
    }

    public async Task<Result<DeleteBasketResponse>> DeleteBasketAsync(string username)
    {
        logger.LogInformation("Deleting basket for user: {Username}", username);
        return await service.DeleteBasketAsync(username);
    }

    public async Task<Result<CheckoutBasketResponse>> CheckoutBasketAsync(CheckoutBasketRequest request)
    {
        logger.LogInformation("Checking out basket for user: {Username}", request.Username);
        return await service.CheckoutBasketAsync(request);
    }
}
