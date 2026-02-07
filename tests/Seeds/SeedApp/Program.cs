using Microsoft.AspNetCore.Identity;
using User.Api.Data;
using User.Api.Data.Models;

Console.WriteLine("===========================================");
Console.WriteLine("MarketSpace Seed Application");
Console.WriteLine("===========================================");
Console.WriteLine("This application will seed data to your REAL databases.");
Console.WriteLine("Make sure your docker-compose services are running:");
Console.WriteLine("  - docker compose up -d");
Console.WriteLine();

// Using real database connections from docker-compose
const string merchantConnectionString =
    "Server=localhost;Port=5436;Database=MerchantDb;User Id=postgres;Password=postgres;Include Error Detail=true";
const string catalogConnectionString =
    "Server=localhost;Port=5432;Database=CatalogDb;User Id=postgres;Password=postgres;Include Error Detail=true";
const string basketConnectionString =
    "Server=localhost;Port=5433;Database=BasketDb;User Id=postgres;Password=postgres;Include Error Detail=true";
const string userConnectionString =
    "Server=localhost;Port=5437;Database=UserDb;User Id=postgres;Password=postgres;Include Error Detail=true";
const string minioEndpoint = "localhost:9000";
const string minioAccessKey = "admin";
const string minioSecretKey = "admin123";

Console.WriteLine("Connecting to databases:");
Console.WriteLine($"  - MerchantDb: localhost:5436");
Console.WriteLine($"  - CatalogDb:  localhost:5432");
Console.WriteLine($"  - BasketDb:   localhost:5433");
Console.WriteLine($"  - UserDb:     localhost:5437");
Console.WriteLine($"  - MinIO:      localhost:9000");
Console.WriteLine();

MarketSpaceSeedFactory factory = new(
    merchantConnectionString: merchantConnectionString,
    catalogConnectionString: catalogConnectionString,
    basketConnectionString: basketConnectionString,
    userConnectionString: userConnectionString,
    minioEndpoint: minioEndpoint,
    minioAccessKey: minioAccessKey,
    minioSecretKey: minioSecretKey
);
IServiceScopeFactory scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

using IServiceScope scope = scopeFactory.CreateScope();
MerchantDbContext merchantDbContext = scope.ServiceProvider.GetRequiredService<MerchantDbContext>();
CatalogDbContext catalogDbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
BasketDbContext basketDbContext = scope.ServiceProvider.GetRequiredService<BasketDbContext>();
UserDbContext userDbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
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

Console.WriteLine("Creating User database schema...");
await userDbContext.Database.EnsureCreatedAsync();
Console.WriteLine("User database schema created successfully!");

var exits = userDbContext.Users.Where(u => u.UserName == "user@example.com");

if (!exits.Any())
{
    ApplicationUser user = new() { UserName = "user@example.com", Email = "Password123!" };
    userDbContext.Users.Add(user);
    await userDbContext.SaveChangesAsync();
}

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
    Console.WriteLine(
        $"Shopping cart created for user: {createdMerchants.ElementAt(i).Name} with {shoppingCart.Items.Count} items");
}

Console.WriteLine("\n✅ Seeding completed successfully!");
Console.WriteLine($"Created {createdMerchants.Count} merchant(s)");
Console.WriteLine($"Created {createdShoppingCarts.Count} shopping cart(s)");
Console.WriteLine("\n===========================================");
Console.WriteLine("Data has been persisted to your databases!");
Console.WriteLine("===========================================");