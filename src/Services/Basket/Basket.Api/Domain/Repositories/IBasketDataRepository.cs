using Basket.Api.Application.Basket.CheckoutBasket.Dtos;
using Basket.Api.Domain.Entities;

namespace Basket.Api.Domain.Repositories;

public interface IBasketDataRepository
{
    Task<ShoppingCartEntity> CreateCartAsync(ShoppingCartEntity cart);
    Task<ShoppingCartEntity?> GetCartAsync(string username);
    Task<bool> CheckoutAsync(string username, CheckoutDataDto checkoutData);
    Task DeleteCartAsync(string username);
}

