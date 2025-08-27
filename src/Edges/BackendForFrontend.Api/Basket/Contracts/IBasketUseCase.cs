using BackendForFrontend.Api.Basket.Dtos;
using BuildingBlocks;

namespace BackendForFrontend.Api.Basket.Contracts;

public interface IBasketUseCase
{
    Task<Result<CreateBasketResponse>> CreateBasketAsync(CreateBasketRequest request);
    Task<Result<GetBasketResponse>> GetBasketByIdAsync(string username);
    Task<Result<DeleteBasketResponse>> DeleteBasketAsync(string username);
    Task<Result<CheckoutBasketResponse>> CheckoutBasketAsync(CheckoutBasketRequest request);
}
