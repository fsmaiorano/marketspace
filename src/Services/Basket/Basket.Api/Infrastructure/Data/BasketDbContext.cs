using Basket.Api.Domain.Entities;
using MongoDB.Driver;

namespace Basket.Api.Infrastructure.Data;

public interface IBasketDbContext
{
    IMongoCollection<ShoppingCartEntity> ShoppingCart { get; }
    IMongoCollection<ShoppingCartItemEntity> ShoppingCartItems { get; }
}

public class BasketDbContext : IBasketDbContext
{
    private readonly IMongoDatabase _database;

    public BasketDbContext(string connectionString, string databaseName)
    {
        MongoClient client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<ShoppingCartEntity> ShoppingCart =>
        _database.GetCollection<ShoppingCartEntity>("ShoppingCartDto");

    public IMongoCollection<ShoppingCartItemEntity> ShoppingCartItems =>
        _database.GetCollection<ShoppingCartItemEntity>("ShoppingCartItems");
}