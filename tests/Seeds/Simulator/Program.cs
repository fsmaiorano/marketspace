using Bogus;
using Simulator;
using System.Net.Http.Json;
using User.Api.Data;
using User.Api.Data.Models;

Console.WriteLine("===========================================");
Console.WriteLine("MarketSpace Simulator Application");
Console.WriteLine("===========================================");

// ======================================
// CONFIGURATION: Checkout simulation settings
// ======================================
const bool doCheckout = true;
const int numberOfItemsInCart = 3; // Number of random catalog items to add to cart
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

// Generate a random user using Bogus
var person = faker.Person;
string randomEmail = person.Email;
string randomUsername = person.UserName;
string randomFirstName = person.FirstName;
string randomLastName = person.LastName;

Console.WriteLine($"🎲 Generating random user for checkout simulation:");
Console.WriteLine($"   Username: {randomUsername}");
Console.WriteLine($"   Email: {randomEmail}");
Console.WriteLine($"   Name: {randomFirstName} {randomLastName}");

// Check if this user already exists, if not create one
ApplicationUser? targetUser = await userDbContext.Users
    .FirstOrDefaultAsync(u => u.Email == randomEmail);

if (targetUser == null)
{
    Console.WriteLine($"✨ Creating new user: {randomUsername}");
    
    targetUser = new ApplicationUser
    {
        Id = Guid.NewGuid().ToString(),
        UserName = randomUsername,
        NormalizedUserName = randomUsername.ToUpper(),
        Email = randomEmail,
        NormalizedEmail = randomEmail.ToUpper(),
        EmailConfirmed = true,
        FirstName = randomFirstName,
        LastName = randomLastName,
        SecurityStamp = Guid.NewGuid().ToString(),
        ConcurrencyStamp = Guid.NewGuid().ToString()
    };
    
    userDbContext.Users.Add(targetUser);
    await userDbContext.SaveChangesAsync();
    Console.WriteLine($"✅ Created new user: {targetUser.UserName} ({targetUser.Email})");
}
else
{
    Console.WriteLine($"✅ Found existing user: {targetUser.UserName} ({targetUser.Email})");
}

// Remove any existing cart for this user to start fresh
ShoppingCartEntity? existingCart = await basketDbContext.ShoppingCarts
    .FirstOrDefaultAsync(sc => sc.Username == targetUser.UserName);

if (existingCart != null)
{
    Console.WriteLine($"🗑️  Removing existing shopping cart for fresh simulation...");
    basketDbContext.ShoppingCarts.Remove(existingCart);
    await basketDbContext.SaveChangesAsync();
}

Console.WriteLine($"🛒 Creating new shopping cart with random catalog items...");

// Get random catalog items to add to the cart
List<CatalogEntity> allCatalogItems = await catalogDbContext.Catalogs.ToListAsync();

if (allCatalogItems.Count == 0)
{
    Console.WriteLine("⚠️  No catalog items found. Cannot proceed with checkout.");
    Console.WriteLine("Please ensure catalog data is seeded first.");
}
else
{
    // Select random catalog items
    var randomCatalogItems = faker.PickRandom(allCatalogItems, Math.Min(numberOfItemsInCart, allCatalogItems.Count)).ToList();
    
    // Create a new shopping cart with random quantities
    var newCart = new ShoppingCartEntity
    {
        Username = targetUser.UserName!,
        Items = randomCatalogItems.Select(c => new ShoppingCartItemEntity
        {
            ProductId = c.Id.Value.ToString(),
            ProductName = c.Name,
            Price = c.Price.Value,
            Quantity = faker.Random.Number(1, 3) // Random quantity between 1 and 3
        }).ToList()
    };

    basketDbContext.ShoppingCarts.Add(newCart);
    await basketDbContext.SaveChangesAsync();
    Console.WriteLine($"✅ Created shopping cart with {newCart.Items.Count} items");

    if (newCart.Items.Count > 0)
    {
        Console.WriteLine("\n📦 Shopping Cart Details:");
        Console.WriteLine($"   User: {newCart.Username}");
        Console.WriteLine($"   Items: {newCart.Items.Count}");
        Console.WriteLine($"   Total Price: ${newCart.TotalPrice:F2}");

        Console.WriteLine("\n   Items in cart:");
        foreach (ShoppingCartItemEntity item in newCart.Items)
        {
            Console.WriteLine(
                $"     - {item.ProductName} (x{item.Quantity}) @ ${item.Price:F2} = ${item.Price * item.Quantity:F2}");
        }

        Console.WriteLine("\n🛒 Ready for checkout!");

        if (doCheckout)
        {
            Console.WriteLine("\n🚀 Proceeding with checkout...");

            try
            {
                using HttpClient httpClient = new();
                httpClient.BaseAddress = new Uri("http://localhost:5001");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var checkoutCommand = new
                {
                    UserName = newCart.Username,
                    CustomerId = targetUser.Id,
                    TotalPrice = newCart.TotalPrice,
                    FirstName = targetUser.FirstName,
                    LastName = targetUser.LastName,
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
                    Coordinates = $"{faker.Address.Latitude()},{faker.Address.Longitude()}"
                };

                Console.WriteLine($"   Posting checkout request to: {httpClient.BaseAddress}/basket/checkout");
                Console.WriteLine($"   Request ID: {checkoutCommand.RequestId}");
                
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