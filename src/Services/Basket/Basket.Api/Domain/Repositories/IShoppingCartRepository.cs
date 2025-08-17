using Basket.Api.Domain.Entities;

namespace Basket.Api.Domain.Repositories;

public interface IShoppingCartRepository
{
    Task<ShoppingCartEntity?> GetCartAsync(string username);
    Task<ShoppingCartEntity> UpdateCartAsync(ShoppingCartEntity cart);
    Task<bool> CheckoutAsync(string username);
    Task DeleteCartAsync(string username);
}