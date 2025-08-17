using Basket.Api.Domain.Entities;

namespace Basket.Api.Domain.Repositories;

public interface IBasketRepository
{
    Task<ShoppingCartEntity> CreateCartAsync(ShoppingCartEntity cart);
    Task<ShoppingCartEntity?> GetCartAsync(string username);
    Task<bool> CheckoutAsync(string username);
    Task DeleteCartAsync(string username);
}