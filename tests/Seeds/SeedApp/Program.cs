﻿using Basket.Api.Domain.Entities;
using Basket.Api.Infrastructure.Data;
using BuildingBlocks.Storage.Minio;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.ValueObjects;
using Catalog.Api.Infrastructure.Data;
using Testcontainers.PostgreSql;
using Testcontainers.Minio;

Console.WriteLine("Starting PostgreSQL container for Basket...");
PostgreSqlContainer basketPostgresContainer = new PostgreSqlBuilder()
    .WithImage("postgres:latest")
    .WithDatabase("BasketDb")
    .WithUsername("postgres")
    .WithPassword("postgres")
    .Build();

await basketPostgresContainer.StartAsync();
Console.WriteLine($"Basket PostgreSQL container started at: {basketPostgresContainer.GetConnectionString()}");

Console.WriteLine("Starting MinIO container...");
MinioContainer minioContainer = new MinioBuilder()
    .WithImage("minio/minio:latest")
    .Build();

await minioContainer.StartAsync();
Console.WriteLine($"MinIO container started at: {minioContainer.GetConnectionString()}");

MarketSpaceSeedFactory factory = new(
    basketConnectionString: basketPostgresContainer.GetConnectionString(),
    minioEndpoint: minioContainer.GetConnectionString().Replace("http://", ""),
    minioAccessKey: MinioBuilder.DefaultUsername,
    minioSecretKey: MinioBuilder.DefaultPassword
);
IServiceScopeFactory scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

using IServiceScope scope = scopeFactory.CreateScope();
MerchantDbContext merchantDbContext = scope.ServiceProvider.GetRequiredService<MerchantDbContext>();
CatalogDbContext catalogDbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
BasketDbContext basketDbContext = scope.ServiceProvider.GetRequiredService<BasketDbContext>();
IMinioBucket minioBucket = scope.ServiceProvider.GetRequiredService<IMinioBucket>();

// Apply migrations
await basketDbContext.Database.EnsureCreatedAsync();


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
for (int i = 0; i < createdMerchants.Count; i++)
{
    ShoppingCartEntity shoppingCart =
        BasketBuilder.CreateShoppingCartFaker(username: createdMerchants.ElementAt(i).Name);
    
    createdShoppingCarts.Add(shoppingCart);
    basketDbContext.ShoppingCarts.Add(shoppingCart);
    var x = await basketDbContext.SaveChangesAsync();
    Console.WriteLine($"Shopping cart created for user: {createdMerchants.ElementAt(i).Name}");
}

Console.WriteLine("\n✅ Seeding completed successfully!");
Console.WriteLine($"Created {createdMerchants.Count} merchant(s)");
Console.WriteLine($"Created {createdShoppingCarts.Count} shopping cart(s)");
Console.WriteLine("\nContainers are still running. Press any key to stop and cleanup...");
Console.ReadKey();

Console.WriteLine("\nStopping containers...");
await basketPostgresContainer.StopAsync();
await minioContainer.StopAsync();
Console.WriteLine("Containers stopped. Goodbye!");
