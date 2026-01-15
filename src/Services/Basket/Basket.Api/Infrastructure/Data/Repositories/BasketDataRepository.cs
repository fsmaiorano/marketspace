using Basket.Api.Domain.Entities;
using Basket.Api.Domain.Repositories;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Basket.Api.Infrastructure.Data.Repositories;

public class BasketDataRepository(IMongoDatabase database, IOptions<DatabaseSettings> settings)
    : IBasketDataRepository
{
    private readonly IMongoCollection<ShoppingCartEntity> _collection = database.GetCollection<ShoppingCartEntity>(settings.Value.CollectionName);

    public async Task<ShoppingCartEntity> CreateCartAsync(ShoppingCartEntity cart)
    {
        await DeleteCartAsync(cart.Username);
        await _collection.InsertOneAsync(cart);
        return cart;
    }

    public async Task<ShoppingCartEntity?> GetCartAsync(string username)
    {
        return await _collection.Find(sc => sc.Username.Equals(username)).FirstOrDefaultAsync();
    }

    public async Task<bool> CheckoutAsync(string username)
    {
        try
        {
            await _collection.DeleteOneAsync(cart => cart.Username == username);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task DeleteCartAsync(string username)
    {
        await _collection.DeleteOneAsync(cart => cart.Username == username);
    }
}