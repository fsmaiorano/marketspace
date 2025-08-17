using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Basket.Api.Domain.Entities;

public class ShoppingCartEntity
{
    // [BsonRepresentation(BsonType.ObjectId)]
    // public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonId] 
    [BsonElement("username")]
    public string Username { get; set; } = null!;

    [BsonElement("items")]
    public List<ShoppingCartItemEntity> Items { get; set; } = new();

    [BsonIgnore]
    public decimal TotalPrice => Items.Sum(item => item.Price * item.Quantity);

    public ShoppingCartEntity(string username)
    {
        Username = username;
    }
}