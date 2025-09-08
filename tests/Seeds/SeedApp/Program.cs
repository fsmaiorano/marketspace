using Basket.Api.Domain.Entities;
using BuildingBlocks.Storage.Minio;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.ValueObjects;
using Catalog.Api.Infrastructure.Data;
using MongoDB.Driver;
using DotNet.Testcontainers.Builders;

MarketSpaceSeedFactory factory = new();
IServiceScopeFactory scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

using IServiceScope scope = scopeFactory.CreateScope();
MerchantDbContext merchantDbContext = scope.ServiceProvider.GetRequiredService<MerchantDbContext>();
CatalogDbContext catalogDbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
IMongoClient basketMongoClient = scope.ServiceProvider.GetRequiredService<IMongoClient>();
IMinioBucket minioBucket = scope.ServiceProvider.GetRequiredService<IMinioBucket>();

new ContainerBuilder()
    .WithImage("minio/minio:latest")
    .WithEnvironment("MINIO_ROOT_USER", "admin")
    .WithEnvironment("MINIO_ROOT_PASSWORD", "admin123")
    .WithPortBinding(9000, true)
    .WithCommand("server", "/data", "--console-address", ":9001")
    .Build();

const int createMerchantCounter = 100;

List<MerchantEntity> createdMerchants = [];
List<ShoppingCartEntity> createdShoppingCarts = [];

// Create merchants
for (int i = 0; i < createMerchantCounter; i++)
{
    MerchantEntity merchant = MerchantBuilder.CreateMerchantFaker().Generate();
    merchant.CreatedBy = "seed";

    MerchantEntity merchantEntity = MerchantEntity.Create(
        merchant.Name,
        merchant.Description,
        merchant.Address,
        merchant.PhoneNumber,
        merchant.Email);

    merchantEntity.Id = merchant.Id;

    merchantDbContext.Merchants.Add(merchantEntity);
    Console.WriteLine("Catalog created: " + merchant.Name + " (" + merchant.Email + ")");
    createdMerchants.Add(merchantEntity);
    merchantDbContext.SaveChanges();
}

// Create catalog
for (int i = 0; i < createdMerchants.Count; i++)
{
    try
    {
        CatalogEntity catalog = CatalogBuilder.CreateCatalogFaker().Generate();
        catalog.CreatedBy = "seed";

        (string objectName, string _) = await minioBucket.SendImageAsync(catalog.ImageUrl);

        CatalogEntity catalogEntity = CatalogEntity.Create(
            name: catalog.Name,
            description: catalog.Description,
            imageUrl: objectName,
            merchantId: createdMerchants.ElementAt(i).Id.Value,
            categories: catalog.Categories,
            price: catalog.Price
        );

        catalogEntity.Id = CatalogId.Of(Guid.CreateVersion7());
        catalogDbContext.Catalogs.Add(catalogEntity);
        catalogDbContext.SaveChanges();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error creating catalog for merchant " + createdMerchants.ElementAt(i).Name + ": " +
                          ex.Message);
    }
}

// Create basket
IMongoDatabase basketDb = basketMongoClient.GetDatabase("BasketDb");
IMongoCollection<ShoppingCartEntity>
    shoppingCartCollection = basketDb.GetCollection<ShoppingCartEntity>("ShoppingCart");

for (int i = 0; i < createdMerchants.Count; i++)
{
    ShoppingCartEntity shoppingCart =
        BasketBuilder.CreateShoppingCartFaker(username: createdMerchants.ElementAt(i).Name);
    createdShoppingCarts.Add(shoppingCart);
    shoppingCartCollection.InsertMany(createdShoppingCarts);
}