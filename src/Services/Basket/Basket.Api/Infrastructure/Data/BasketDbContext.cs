using Basket.Api.Domain.Entities;
using MongoDB.Driver;

namespace Basket.Api.Infrastructure.Data;

public class BasketDbContext
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