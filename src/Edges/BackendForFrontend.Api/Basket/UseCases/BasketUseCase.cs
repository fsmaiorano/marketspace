using BackendForFrontend.Api.Basket.Dtos;
using BackendForFrontend.Api.Basket.Services;
using BuildingBlocks;
using BuildingBlocks.Loggers;

namespace BackendForFrontend.Api.Basket.UseCases;

public interface IBasketUseCase
{
    Task<Result<CreateBasketResponse>> CreateBasketAsync(CreateBasketRequest request);
    Task<Result<GetBasketResponse>> GetBasketByIdAsync(string username);
    Task<Result<DeleteBasketResponse>> DeleteBasketAsync(string username);
    Task<Result<CheckoutBasketResponse>> CheckoutBasketAsync(CheckoutBasketRequest request);
}

public class BasketUseCase(
    IAppLogger<BasketUseCase> logger,
    IBasketService service) : IBasketUseCase
{
    public async Task<Result<CreateBasketResponse>> CreateBasketAsync(CreateBasketRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Creating basket for user: {Username}", request.Username);
        return await service.CreateBasketAsync(request);
    }

    public async Task<Result<GetBasketResponse>> GetBasketByIdAsync(string username)
    {
        logger.LogInformation(LogTypeEnum.Application, "Retrieving basket for user: {Username}", username);
        return await service.GetBasketByIdAsync(username);
    }

    public async Task<Result<DeleteBasketResponse>> DeleteBasketAsync(string username)
    {
        logger.LogInformation(LogTypeEnum.Application, "Deleting basket for user: {Username}", username);
        return await service.DeleteBasketAsync(username);
    }

    public async Task<Result<CheckoutBasketResponse>> CheckoutBasketAsync(CheckoutBasketRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Checking out basket for user: {Username}", request.Username);
        return await service.CheckoutBasketAsync(request);
    }
}
