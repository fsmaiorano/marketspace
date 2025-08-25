using BackendForFrontend.Api.Basket.Dtos;

namespace BackendForFrontend.Api.Basket.Contracts;

public interface IBasketUseCase
{
    Task<CreateBasketResponse> CreateBasketAsync(CreateBasketRequest request);
    Task<GetBasketResponse> GetBasketByIdAsync(string username);
    Task<DeleteBasketResponse> DeleteBasketAsync(string username);
    Task<CheckoutBasketResponse> CheckoutBasketAsync(CheckoutBasketRequest request);
}
