using Basket.Api.Domain.Entities;
using Basket.Api.Domain.ValueObjects;

namespace Basket.Api.Domain.Repositories;

public interface IBasketDataRepository
{
    Task<ShoppingCartEntity> CreateCartAsync(ShoppingCartEntity cart);
    Task<ShoppingCartEntity?> GetCartAsync(string username);
    Task<bool> CheckoutAsync(string username, CheckoutData checkoutData);
    Task DeleteCartAsync(string username);
}

