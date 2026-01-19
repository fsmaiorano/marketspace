using Basket.Api.Domain.Entities;
using BuildingBlocks.Storage.Minio;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.ValueObjects;
using Catalog.Api.Infrastructure.Data;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Testcontainers.Minio;

Console.WriteLine("Starting MongoDB container...");
MongoDbContainer mongoDbContainer = new MongoDbBuilder()
    .WithImage("mongo:latest")
    .Build();

await mongoDbContainer.StartAsync();
Console.WriteLine($"MongoDB container started at: {mongoDbContainer.GetConnectionString()}");

Console.WriteLine("Starting MinIO container...");
MinioContainer minioContainer = new MinioBuilder()
    .WithImage("minio/minio:latest")
    .Build();

await minioContainer.StartAsync();
Console.WriteLine($"MinIO container started at: {minioContainer.GetConnectionString()}");

MarketSpaceSeedFactory factory = new(
    mongoConnectionString: mongoDbContainer.GetConnectionString(),
    minioEndpoint: minioContainer.GetConnectionString().Replace("http://", ""),
    minioAccessKey: MinioBuilder.DefaultUsername,
    minioSecretKey: MinioBuilder.DefaultPassword
);
IServiceScopeFactory scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

using IServiceScope scope = scopeFactory.CreateScope();
MerchantDbContext merchantDbContext = scope.ServiceProvider.GetRequiredService<MerchantDbContext>();
CatalogDbContext catalogDbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
IMongoClient basketMongoClient = scope.ServiceProvider.GetRequiredService<IMongoClient>();
IMinioBucket minioBucket = scope.ServiceProvider.GetRequiredService<IMinioBucket>();


const int createMerchantCounter = 1;

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
var basketDb = basketMongoClient.GetDatabase("BasketDb");
IMongoCollection<ShoppingCartEntity>
    shoppingCartCollection = basketDb.GetCollection<ShoppingCartEntity>("ShoppingCart");

for (int i = 0; i < createdMerchants.Count; i++)
{
    ShoppingCartEntity shoppingCart =
        BasketBuilder.CreateShoppingCartFaker(username: createdMerchants.ElementAt(i).Name);
    
    createdShoppingCarts.Add(shoppingCart);
    await shoppingCartCollection.InsertOneAsync(shoppingCart);
    Console.WriteLine($"Shopping cart created for user: {createdMerchants.ElementAt(i).Name}");
}

Console.WriteLine("\n✅ Seeding completed successfully!");
Console.WriteLine($"Created {createdMerchants.Count} merchant(s)");
Console.WriteLine($"Created {createdShoppingCarts.Count} shopping cart(s)");
Console.WriteLine("\nContainers are still running. Press any key to stop and cleanup...");
Console.ReadKey();

Console.WriteLine("\nStopping containers...");
await mongoDbContainer.StopAsync();
await minioContainer.StopAsync();
Console.WriteLine("Containers stopped. Goodbye!");
