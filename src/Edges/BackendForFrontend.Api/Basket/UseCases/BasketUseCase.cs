using BackendForFrontend.Api.Basket.Dtos;
using BackendForFrontend.Api.Basket.Services;
using BuildingBlocks;
using BuildingBlocks.Loggers.Abstractions;

namespace BackendForFrontend.Api.Basket.UseCases;

public interface IBasketUseCase
{
    Task<Result<CreateBasketResponse>> CreateBasketAsync(CreateBasketRequest request);
    Task<Result<GetBasketResponse>> GetBasketByIdAsync(string username);
    Task<Result<DeleteBasketResponse>> DeleteBasketAsync(string username);
    Task<Result<CheckoutBasketResponse>> CheckoutBasketAsync(CheckoutBasketRequest request);
}

public class BasketUseCase(
    IApplicationLogger<BasketUseCase> applicationLogger,
    IBusinessLogger<BasketUseCase> businessLogger,
    IBasketService service) : IBasketUseCase
{
    public async Task<Result<CreateBasketResponse>> CreateBasketAsync(CreateBasketRequest request)
    {
        applicationLogger.LogInformation("Creating basket for user: {Username}", request.Username);
        return await service.CreateBasketAsync(request);
    }

    public async Task<Result<GetBasketResponse>> GetBasketByIdAsync(string username)
    {
        applicationLogger.LogInformation("Retrieving basket for user: {Username}", username);
        return await service.GetBasketByIdAsync(username);
    }

    public async Task<Result<DeleteBasketResponse>> DeleteBasketAsync(string username)
    {
        applicationLogger.LogInformation("Deleting basket for user: {Username}", username);
        return await service.DeleteBasketAsync(username);
    }

    public async Task<Result<CheckoutBasketResponse>> CheckoutBasketAsync(CheckoutBasketRequest request)
    {
        applicationLogger.LogInformation("Checking out basket for user: {Username}", request.Username);
        return await service.CheckoutBasketAsync(request);
    }
}
