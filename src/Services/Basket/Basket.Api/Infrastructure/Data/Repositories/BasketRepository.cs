using Basket.Api.Domain.Entities;
using Basket.Api.Domain.Repositories;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Basket.Api.Infrastructure.Data.Repositories;

public class BasketRepository : IBasketRepository
{
    private readonly IMongoCollection<ShoppingCartEntity> _collection;

    public BasketRepository(IOptions<DatabaseSettings> settings)
    {
        MongoClient client = new MongoClient(settings.Value.ConnectionString);
        IMongoDatabase? database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<ShoppingCartEntity>(settings.Value.CollectionName);
    }

    public async Task<ShoppingCartEntity> CreateCartAsync(ShoppingCartEntity cart)
    {
        await DeleteCartAsync(cart.Username); 
        await _collection.InsertOneAsync(cart);
        return cart;
    }

    public async Task<ShoppingCartEntity?> GetCartAsync(string username)
    {
        return await _collection
            .Find(cart => cart.Username == username)
            .FirstOrDefaultAsync();
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