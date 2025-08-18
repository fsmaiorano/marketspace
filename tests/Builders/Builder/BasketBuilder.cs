using Basket.Api.Application.Basket.CreateBasket;
using Basket.Api.Application.Basket.DeleteBasket;
using Basket.Api.Application.Dto;
using Basket.Api.Domain.Entities;
using Bogus;

namespace Builder;

public static class BasketBuilder
{
    private static readonly string[] ProductNames =
    {
        "Wireless Headphones", "Gaming Mouse", "Coffee Mug", "Bluetooth Speaker", "Phone Case", 
        "Laptop Stand", "Water Bottle", "Desk Lamp", "Notebook", "Pen Set", "USB Cable", 
        "Power Bank", "Screen Protector", "Keyboard", "Mouse Pad", "Tablet", "Camera", "Backpack"
    };

    public static Faker<ShoppingCartEntity> CreateShoppingCartFaker(string username = "")
    {
        return new Faker<ShoppingCartEntity>()
            .CustomInstantiator(f => new ShoppingCartEntity
            {
                Username = !string.IsNullOrEmpty(username) ? username : f.Internet.UserName(),
                Items = CreateShoppingCartItemFaker().Generate(f.Random.Int(1, 5))
            });
    }

    public static Faker<ShoppingCartItemEntity> CreateShoppingCartItemFaker()
    {
        return new Faker<ShoppingCartItemEntity>()
            .CustomInstantiator(f => new ShoppingCartItemEntity
            {
                ProductName = f.PickRandom(ProductNames),
                Price = f.Finance.Amount(1, 500, 2),
                Quantity = f.Random.Int(1, 10)
            });
    }

    public static Faker<CreateBasketCommand> CreateBasketCommandFaker(string username = "")
    {
        return new Faker<CreateBasketCommand>()
            .CustomInstantiator(f => new CreateBasketCommand
            {
                Username = !string.IsNullOrEmpty(username) ? username : f.Internet.UserName(),
                Items = CreateShoppingCartItemDtoFaker().Generate(f.Random.Int(1, 3))
            });
    }

    public static Faker<DeleteBasketCommand> CreateDeleteBasketCommandFaker(string username = "")
    {
        return new Faker<DeleteBasketCommand>()
            .CustomInstantiator(f => new DeleteBasketCommand
            {
                Username = !string.IsNullOrEmpty(username) ? username : f.Internet.UserName()
            });
    }

    public static Faker<ShoppingCartItemDto> CreateShoppingCartItemDtoFaker()
    {
        return new Faker<ShoppingCartItemDto>()
            .CustomInstantiator(f => new ShoppingCartItemDto
            {
                ProductName = f.PickRandom(ProductNames),
                Price = f.Finance.Amount(1, 500, 2),
                Quantity = f.Random.Int(1, 10)
            });
    }

    public static Faker<ShoppingCartDto> CreateShoppingCartDtoFaker(string username = "")
    {
        return new Faker<ShoppingCartDto>()
            .CustomInstantiator(f => new ShoppingCartDto
            {
                Username = !string.IsNullOrEmpty(username) ? username : f.Internet.UserName(),
                Items = CreateShoppingCartItemDtoFaker().Generate(f.Random.Int(1, 5))
            });
    }
}
