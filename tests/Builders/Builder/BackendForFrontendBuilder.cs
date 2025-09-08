using BackendForFrontend.Api.Merchant.Dtos;
using BackendForFrontend.Api.Basket.Dtos;
using BackendForFrontend.Api.Catalog.Dtos;
using BackendForFrontend.Api.Order.Dtos;
using Bogus;

namespace Builder;

public class BackendForFrontendBuilder
{
    // Merchant Builders
    public static CreateMerchantRequest CreateMerchantRequestFaker()
    {
        return new Faker<CreateMerchantRequest>().CustomInstantiator(f => new CreateMerchantRequest
        {
            Name = f.Company.CompanyName(),
            Description = f.Lorem.Sentence(),
            Address = f.Address.FullAddress(),
            PhoneNumber = f.Phone.PhoneNumber(),
            Email = f.Internet.Email()
        });
    }

    // Basket Builders
    public static CreateBasketRequest CreateBasketRequestFaker()
    {
        return new Faker<CreateBasketRequest>().CustomInstantiator(f => new CreateBasketRequest
        {
            Username = f.Internet.UserName(), Items = CreateBasketItemFaker().Generate(f.Random.Int(1, 3))
        });
    }

    public static Faker<BasketItemDto> CreateBasketItemFaker()
    {
        return new Faker<BasketItemDto>().CustomInstantiator(f => new BasketItemDto
        {
            Quantity = f.Random.Int(1, 5),
            Price = f.Random.Decimal(10, 100),
            ProductId = Guid.NewGuid().ToString(),
            ProductName = f.Commerce.ProductName()
        });
    }

    public static CheckoutBasketRequest CreateCheckoutBasketRequestFaker()
    {
        return new Faker<CheckoutBasketRequest>().CustomInstantiator(f => new CheckoutBasketRequest
        {
            Username = f.Internet.UserName(),
            FirstName = f.Name.FirstName(),
            LastName = f.Name.LastName(),
            EmailAddress = f.Internet.Email(),
            AddressLine = f.Address.StreetAddress(),
            Country = f.Address.Country(),
            State = f.Address.State(),
            ZipCode = f.Address.ZipCode(),
            CardName = f.Name.FullName(),
            CardNumber = f.Finance.CreditCardNumber(),
            Expiration = "12/25",
            CVV = f.Finance.Random.String2(3, "0123456789"),
            PaymentMethod = f.Random.Int(1, 3)
        });
    }

    // Catalog Builders
    public static CreateCatalogRequest CreateCatalogRequestFaker()
    {
        return new Faker<CreateCatalogRequest>().CustomInstantiator(f => new CreateCatalogRequest
        {
            Name = f.Commerce.ProductName(),
            Description = f.Commerce.ProductDescription(),
            Price = Math.Round(f.Random.Decimal(100, 1000), 2),
            Categories = f.Commerce.Categories(1).ToList(),
            ImageUrl = "https://via.placeholder.com/300x300.jpg", // Use a reliable placeholder service
            MerchantId = f.Random.Guid()
        });
    }

    public static UpdateCatalogRequest CreateUpdateCatalogRequestFaker()
    {
        return new Faker<UpdateCatalogRequest>().CustomInstantiator(f => new UpdateCatalogRequest
        {
            Id = f.Random.Guid(),
            Name = f.Commerce.ProductName(),
            Description = f.Commerce.ProductDescription(),
            Price = Math.Round(f.Random.Decimal(100, 1000), 2),
            Categories = f.Commerce.Categories(1).ToList(),
            ImageUrl = "https://via.placeholder.com/300x300.jpg", // Use a reliable placeholder service
            MerchantId = f.Random.Guid()
        });
    }

    // Order Builders
    public static CreateOrderRequest CreateOrderRequestFaker()
    {
        return new Faker<CreateOrderRequest>().CustomInstantiator(f => new CreateOrderRequest
        {
            CustomerId = f.Random.Guid(),
            ShippingAddress = CreateAddressDtoFaker().Generate(),
            BillingAddress = CreateAddressDtoFaker().Generate(),
            Payment = CreatePaymentDtoFaker().Generate(),
            Items = CreateOrderItemDtoFaker().Generate(f.Random.Int(1, 3))
        });
    }

    public static UpdateOrderRequest CreateUpdateOrderRequestFaker()
    {
        return new Faker<UpdateOrderRequest>().CustomInstantiator(f => new UpdateOrderRequest
        {
            Id = f.Random.Guid(),
            CustomerId = f.Random.Guid(),
            ShippingAddress = CreateAddressDtoFaker().Generate(),
            BillingAddress = CreateAddressDtoFaker().Generate(),
            Payment = CreatePaymentDtoFaker().Generate(),
            Status = f.Random.Int(1, 5),
            Items = CreateOrderItemDtoFaker().Generate(f.Random.Int(1, 3)),
            TotalAmount = f.Random.Decimal(50, 500)
        });
    }

    public static Faker<AddressDto> CreateAddressDtoFaker()
    {
        return new Faker<AddressDto>().CustomInstantiator(f => new AddressDto
        {
            FirstName = f.Name.FirstName(),
            LastName = f.Name.LastName(),
            EmailAddress = f.Internet.Email(),
            AddressLine = f.Address.StreetAddress(),
            Country = f.Address.Country(),
            State = f.Address.State(),
            ZipCode = f.Address.ZipCode()
        });
    }

    public static Faker<PaymentDto> CreatePaymentDtoFaker()
    {
        return new Faker<PaymentDto>().CustomInstantiator(f => new PaymentDto
        {
            CardName = f.Name.FullName(),
            CardNumber = f.Finance.CreditCardNumber(),
            Expiration = "12/25",
            CVV = f.Finance.Random.String2(3, "0123456789"),
            PaymentMethod = f.Random.Int(1, 3)
        });
    }

    public static Faker<OrderItemDto> CreateOrderItemDtoFaker()
    {
        return new Faker<OrderItemDto>().CustomInstantiator(f => new OrderItemDto
        {
            ProductId = f.Random.Guid(), Quantity = f.Random.Int(1, 5), Price = f.Random.Decimal(10, 100)
        });
    }
}