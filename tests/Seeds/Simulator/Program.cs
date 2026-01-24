using Bogus;
using Simulator;
using System.Net.Http.Json;
using User.Api.Data;
using User.Api.Data.Models;

Console.WriteLine("===========================================");
Console.WriteLine("MarketSpace Simulator Application");
Console.WriteLine("===========================================");

// ======================================
// CONFIGURATION: Set the user email for checkout simulation
// ======================================
const string targetUserEmail = "user@example.com";
const bool doCheckout = true;
// ======================================

Faker faker = new Bogus.Faker();

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

MarketSpaceSimulatorFactory factory = new(
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

// ======================================
// Checkout Simulation
// ======================================
Console.WriteLine("\n===========================================");
Console.WriteLine("Starting Checkout Simulation");
Console.WriteLine("===========================================");

// Find the specific merchant/user by email
ApplicationUser? targetUser = await userDbContext.Users
    .FirstOrDefaultAsync(m => m.Email == targetUserEmail);

if (targetUser == null)
{
    Console.WriteLine($"⚠️  Warning: No merchant found with email '{targetUserEmail}'");
    Console.WriteLine("Please update the 'targetUserEmail' variable to match an existing merchant's email.");
    Console.WriteLine("\nAvailable merchant emails:");
    List<MerchantEntity> allMerchants = await merchantDbContext.Merchants.Take(5).ToListAsync();
    foreach (MerchantEntity merchant in allMerchants)
    {
        Console.WriteLine($"  - {merchant.Email} ({merchant.Name})");
    }
}
else
{
    Console.WriteLine($"✅ Found target merchant: {targetUser.UserName} ({targetUser.Email})");

    // Check if a shopping cart exists for this user
    ShoppingCartEntity? existingCart = await basketDbContext.ShoppingCarts
        .FirstOrDefaultAsync(sc => sc.Username == targetUser.UserName);

    if (existingCart == null)
    {
        Console.WriteLine($"⚠️  No shopping cart found for user: {targetUser.UserName}");
        Console.WriteLine("Creating a new shopping cart with catalog items...");

        // Get some catalog items to add to the cart
        List<CatalogEntity> catalogItems = await catalogDbContext.Catalogs
            .Take(3)
            .ToListAsync();

        if (catalogItems.Count == 0)
        {
            Console.WriteLine("⚠️  No catalog items found for this merchant. Cannot proceed with checkout.");
        }
        else
        {
            // Create a new shopping cart
            existingCart = new ShoppingCartEntity
            {
                Username = targetUser.UserName!,
                Items = catalogItems.Select(c => new ShoppingCartItemEntity
                {
                    ProductId = c.Id.Value.ToString(), ProductName = c.Name, Price = c.Price.Value, Quantity = 1
                }).ToList()
            };

            basketDbContext.ShoppingCarts.Add(existingCart);
            await basketDbContext.SaveChangesAsync();
            Console.WriteLine($"✅ Created shopping cart with {existingCart.Items.Count} items");
        }
    }
    else
    {
        Console.WriteLine($"✅ Found existing shopping cart with {existingCart.Items.Count} items");
    }

    if (existingCart is { Items.Count: > 0 })
    {
        Console.WriteLine("\n📦 Shopping Cart Details:");
        Console.WriteLine($"   User: {existingCart.Username}");
        Console.WriteLine($"   Items: {existingCart.Items.Count}");
        Console.WriteLine($"   Total Price: ${existingCart.TotalPrice:F2}");

        Console.WriteLine("\n   Items in cart:");
        foreach (ShoppingCartItemEntity item in existingCart.Items)
        {
            Console.WriteLine(
                $"     - {item.ProductName} (x{item.Quantity}) @ ${item.Price:F2} = ${item.Price * item.Quantity:F2}");
        }

        Console.WriteLine("\n🛒 Ready for checkout!");
        Console.WriteLine("To proceed with checkout, you would need to:");
        Console.WriteLine("  1. Use the Basket API endpoint: POST /basket/checkout");
        Console.WriteLine($"  2. With username: {existingCart.Username}");
        Console.WriteLine($"  3. Total amount: ${existingCart.TotalPrice:F2}");
        Console.WriteLine("\nThe shopping cart is now persisted in the database and ready for checkout via the API.");

        if (doCheckout)
        {
            Console.WriteLine("\n🚀 Proceeding with checkout...");

            try
            {
                using HttpClient httpClient = new();
                httpClient.BaseAddress = new Uri("http://localhost:5001");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                if (string.IsNullOrWhiteSpace(targetUser.FirstName) || string.IsNullOrWhiteSpace(targetUser.LastName))
                {
                    targetUser.FirstName = faker.Person.FirstName;
                    targetUser.LastName = faker.Person.LastName;
                    await userDbContext.SaveChangesAsync();
                }

                var checkoutCommand = new
                {
                    UserName = existingCart.Username,
                    CustomerId = targetUser.Id,
                    TotalPrice = existingCart.TotalPrice,
                    FirstName = targetUser.FirstName ?? "John",
                    LastName = targetUser.LastName ?? "Doe",
                    EmailAddress = targetUser.Email,
                    AddressLine = faker.Address.StreetAddress(),
                    Country = faker.Address.Country(),
                    State = faker.Address.State(),
                    ZipCode = faker.Address.ZipCode(),
                    CardName = $"{targetUser.FirstName} {targetUser.LastName}",
                    CardNumber = faker.Finance.CreditCardNumber(),
                    Expiration = faker.Date.Future().ToString("MM/yy"),
                    Cvv = faker.Random.Number(100, 999).ToString(),
                    PaymentMethod = 1,
                    RequestId = Guid.NewGuid().ToString(),
                    IdempotencyKey = Guid.NewGuid().ToString()
                };

                Console.WriteLine($"   Posting checkout request to: {httpClient.BaseAddress}/basket/checkout");
                HttpResponseMessage response = await httpClient.PostAsJsonAsync("/basket/checkout", checkoutCommand);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("✅ Checkout completed successfully!");
                    Console.WriteLine($"   Response: {responseContent}");
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Checkout failed with status code: {response.StatusCode}");
                    Console.WriteLine($"   Error: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception during checkout: {ex.Message}");
                Console.WriteLine($"   Make sure the Basket API is running on http://localhost:5001");
            }
        }
    }
}

Console.WriteLine("\n===========================================");
Console.WriteLine("Data has been persisted to your databases!");
Console.WriteLine("===========================================");