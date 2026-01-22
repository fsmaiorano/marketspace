﻿using Basket.Api.Domain.Entities;
using Basket.Api.Infrastructure.Data;
using BuildingBlocks.Storage.Minio;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.ValueObjects;
using Catalog.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("===========================================");
Console.WriteLine("MarketSpace Seed Application");
Console.WriteLine("===========================================");
Console.WriteLine("This application will seed data to your REAL databases.");
Console.WriteLine("Make sure your docker-compose services are running:");
Console.WriteLine("  - docker compose up -d");
Console.WriteLine();

// Using real database connections from docker-compose
const string merchantConnectionString = "Server=localhost;Port=5436;Database=MerchantDb;User Id=postgres;Password=postgres;Include Error Detail=true";
const string catalogConnectionString = "Server=localhost;Port=5432;Database=CatalogDb;User Id=postgres;Password=postgres;Include Error Detail=true";
const string basketConnectionString = "Server=localhost;Port=5433;Database=BasketDb;User Id=postgres;Password=postgres;Include Error Detail=true";
const string minioEndpoint = "localhost:9000";
const string minioAccessKey = "admin";
const string minioSecretKey = "admin123";

Console.WriteLine("Connecting to databases:");
Console.WriteLine($"  - MerchantDb: localhost:5436");
Console.WriteLine($"  - CatalogDb:  localhost:5432");
Console.WriteLine($"  - BasketDb:   localhost:5433");
Console.WriteLine($"  - MinIO:      localhost:9000");
Console.WriteLine();

MarketSpaceSeedFactory factory = new(
    merchantConnectionString: merchantConnectionString,
    catalogConnectionString: catalogConnectionString,
    basketConnectionString: basketConnectionString,
    minioEndpoint: minioEndpoint,
    minioAccessKey: minioAccessKey,
    minioSecretKey: minioSecretKey
);
IServiceScopeFactory scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

using IServiceScope scope = scopeFactory.CreateScope();
MerchantDbContext merchantDbContext = scope.ServiceProvider.GetRequiredService<MerchantDbContext>();
CatalogDbContext catalogDbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
BasketDbContext basketDbContext = scope.ServiceProvider.GetRequiredService<BasketDbContext>();
IMinioBucket minioBucket = scope.ServiceProvider.GetRequiredService<IMinioBucket>();

// Create database schemas
Console.WriteLine("Creating Merchant database schema...");
await merchantDbContext.Database.EnsureCreatedAsync();
Console.WriteLine("Merchant database schema created successfully!");

Console.WriteLine("Creating Catalog database schema...");
await catalogDbContext.Database.EnsureCreatedAsync();
Console.WriteLine("Catalog database schema created successfully!");

Console.WriteLine("Creating Basket database schema...");
await basketDbContext.Database.EnsureCreatedAsync();
Console.WriteLine("Basket database schema created successfully!");


const int createMerchantCounter = 20;

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
Console.WriteLine("\nCreating shopping carts...");
for (int i = 0; i < createdMerchants.Count; i++)
{
    ShoppingCartEntity shoppingCart =
        BasketBuilder.CreateShoppingCartFaker(username: createdMerchants.ElementAt(i).Name);
    
    createdShoppingCarts.Add(shoppingCart);
    basketDbContext.ShoppingCarts.Add(shoppingCart);
    await basketDbContext.SaveChangesAsync();
    Console.WriteLine($"Shopping cart created for user: {createdMerchants.ElementAt(i).Name} with {shoppingCart.Items.Count} items");
}

// Verify basket data was saved
Console.WriteLine("\nVerifying basket data in database...");
int basketCount = await basketDbContext.ShoppingCarts.CountAsync();
Console.WriteLine($"Total shopping carts in database: {basketCount}");

if (basketCount > 0)
{
    Console.WriteLine("\nShopping cart details:");
    var allCarts = await basketDbContext.ShoppingCarts.ToListAsync();
    foreach (var cart in allCarts)
    {
        Console.WriteLine($"  - User: {cart.Username}, Items: {cart.Items?.Count ?? 0}, Total: ${cart.TotalPrice:F2}");
    }
}

Console.WriteLine("\n✅ Seeding completed successfully!");
Console.WriteLine($"Created {createdMerchants.Count} merchant(s)");
Console.WriteLine($"Created {createdShoppingCarts.Count} shopping cart(s)");
Console.WriteLine($"Verified {basketCount} shopping cart(s) in database");
Console.WriteLine("\n===========================================");
Console.WriteLine("Data has been persisted to your databases!");
Console.WriteLine("===========================================");
