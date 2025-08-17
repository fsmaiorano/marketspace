using MongoDB.Bson.Serialization.Attributes;

namespace Basket.Api.Domain.Entities;

public class ShoppingCartItemEntity
{
    [BsonElement("quantity")]
    public int Quantity { get; set; }
    
    [BsonElement("price")]
    public decimal Price { get; set; }

    [BsonElement("productName")]
    public string ProductName { get; set; } = null!;
}