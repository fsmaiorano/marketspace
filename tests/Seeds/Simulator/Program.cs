using Bogus;
using Merchant.Api.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Simulator;
using System.Net.Http.Json;
using System.Text.Json;
using User.Api.Data;
using User.Api.Models;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("===========================================");
Console.WriteLine("   MarketSpace Simulator Application");
Console.WriteLine("===========================================");
Console.WriteLine();

// ======================================
// CONFIGURATION
// ======================================
const string bffBaseUrl = "https://localhost:5150"; // ajuste para a porta do seu BFF
const string basketApiBaseUrl = "http://localhost:5001";

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
// ======================================

// ── Main menu ──────────────────────────────────────────────────────────
bool exit = false;
while (!exit)
{
    Console.WriteLine();
    Console.WriteLine("Choose simulation mode:");
    Console.WriteLine("  [0] Seed             – seeds initial data (merchants, catalogs, carts)");
    Console.WriteLine("  [1] Direct DB Access – accesses DBs directly (internal logic, varied data)");
    Console.WriteLine("  [2] Via BFF HTTP     – makes real calls to your BFF");
    Console.WriteLine("  [3] Both            – executes both modes in sequence");
    Console.WriteLine("  [4] Exit");
    Console.WriteLine("  [5] DEMO             – full end-to-end demo: merchant creates products, customer buys them");
    Console.Write("> ");

    string? choice = Console.ReadLine()?.Trim();
    Console.WriteLine();

    switch (choice)
    {
        case "0":
            await RunSeedModeAsync();
            break;
        case "1":
            Console.Write("How many records should be created? [1]: ");
            string? input = Console.ReadLine()?.Trim();
            int recordsToCreate = 1;
            if (!string.IsNullOrEmpty(input) && int.TryParse(input, out int n) && n > 0)
                recordsToCreate = n;
            await RunDirectDbModeAsync(recordsToCreate);
            break;
        case "2":
            await RunBffHttpModeAsync();
            break;
        case "3":
            Console.Write("How many records should be created? [1]: ");
            input = Console.ReadLine()?.Trim();
            recordsToCreate = 1;
            if (!string.IsNullOrEmpty(input) && int.TryParse(input, out n) && n > 0)
                recordsToCreate = n;
            await RunDirectDbModeAsync(recordsToCreate);
            await RunBffHttpModeAsync();
            break;
        case "4":
            exit = true;
            break;
        case "5":
            await RunDemoModeAsync();
            break;
        default:
            Console.WriteLine("Invalid option, please try again.");
            break;
    }
}

Console.WriteLine("Simulator finished. Goodbye!");

// ── MODE 0 – Seed application ─────────────────────────────────────────
async Task RunSeedModeAsync()
{
    Console.WriteLine("===========================================");
    Console.WriteLine(" MODO 0 – Seed Application");
    Console.WriteLine("===========================================");

    // Ask for number of merchants
    Console.Write("\nHow many merchants should be created? [20]: ");
    string? merchantInput = Console.ReadLine()?.Trim();
    int merchantCount = 20;
    if (!string.IsNullOrEmpty(merchantInput) && int.TryParse(merchantInput, out int m) && m > 0)
        merchantCount = m;

    // Ask for number of catalogs per merchant
    Console.Write("How many catalogs per merchant? [1]: ");
    string? catalogInput = Console.ReadLine()?.Trim();
    int catalogsPerMerchant = 1;
    if (!string.IsNullOrEmpty(catalogInput) && int.TryParse(catalogInput, out int c) && c > 0)
        catalogsPerMerchant = c;

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
    UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    IMinioBucket minioBucket = scope.ServiceProvider.GetRequiredService<IMinioBucket>();

    Console.WriteLine("Creating schemas...");
    await merchantDbContext.Database.EnsureCreatedAsync();
    await catalogDbContext.Database.EnsureCreatedAsync();
    await basketDbContext.Database.EnsureCreatedAsync();
    await userDbContext.Database.EnsureCreatedAsync();
    Console.WriteLine("Schemas OK.");

    // Create custom merchant and customer users
    Console.WriteLine("\n📝 Creating custom users...");
    
    // Check if merchant user already exists
    ApplicationUser? existingMerchantUser = await userManager.FindByEmailAsync("merchant@marketspace.com");
    
    if (existingMerchantUser == null)
    {
        existingMerchantUser = new ApplicationUser
        {
            UserName = "merchant@marketspace.com",
            Email = "merchant@marketspace.com",
            EmailConfirmed = true,
            Name = "Merchant User",
            UserType = UserTypeEnum.Merchant
        };
        
        IdentityResult result = await userManager.CreateAsync(existingMerchantUser, "123456");
        if (result.Succeeded)
        {
            // Ensure "Member" role exists and assign it
            if (!await roleManager.RoleExistsAsync("Member"))
            {
                await roleManager.CreateAsync(new IdentityRole("Member"));
            }
            await userManager.AddToRoleAsync(existingMerchantUser, "Member");
            Console.WriteLine($"✅ Merchant user created: {existingMerchantUser.Email} (ID: {existingMerchantUser.Id})");
        }
        else
        {
            Console.WriteLine($"❌ Failed to create merchant user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        Console.WriteLine($"⚠️  Merchant user already exists: {existingMerchantUser.Email} (ID: {existingMerchantUser.Id})");
    }

    // Check if customer user already exists
    ApplicationUser? existingCustomerUser = await userManager.FindByEmailAsync("customer@marketspace.com");
    
    if (existingCustomerUser == null)
    {
        existingCustomerUser = new ApplicationUser
        {
            UserName = "customer@marketspace.com",
            Email = "customer@marketspace.com",
            EmailConfirmed = true,
            Name = "Customer User",
            UserType = UserTypeEnum.Customer
        };
        
        IdentityResult result = await userManager.CreateAsync(existingCustomerUser, "123456");
        if (result.Succeeded)
        {
            // Ensure "Member" role exists and assign it
            if (!await roleManager.RoleExistsAsync("Member"))
            {
                await roleManager.CreateAsync(new IdentityRole("Member"));
            }
            await userManager.AddToRoleAsync(existingCustomerUser, "Member");
            Console.WriteLine($"✅ Customer user created: {existingCustomerUser.Email} (ID: {existingCustomerUser.Id})");
        }
        else
        {
            Console.WriteLine($"❌ Failed to create customer user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        Console.WriteLine($"⚠️  Customer user already exists: {existingCustomerUser.Email} (ID: {existingCustomerUser.Id})");
    }

    Console.WriteLine("\n👨‍💼 Setting up merchant user...");
    Guid merchantUserId = Guid.Parse(existingMerchantUser.Id);
    
    MerchantEntity? existingMerchant = null;
    try
    {
        List<MerchantRawDto> merchantsRaw = await merchantDbContext.Database
            .SqlQueryRaw<MerchantRawDto>(
                "SELECT \"Id\", \"UserId\", \"Name\", \"Email\" FROM \"Merchants\" WHERE \"UserId\" = {0}",
                merchantUserId)
            .ToListAsync();
        
        if (merchantsRaw.Any())
        {
            Guid merchantId = merchantsRaw.First().Id;
            existingMerchant = await merchantDbContext.Merchants
                .FirstOrDefaultAsync(m => m.Id == MerchantId.Of(merchantId));
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠️  Could not query existing merchants: {ex.Message}");
    }
    
    if (existingMerchant == null)
    {
        MerchantEntity newMerchant = MerchantBuilder.CreateMerchantFaker().Generate();
        newMerchant.CreatedBy = "seed";

        MerchantEntity merchantEntity = MerchantEntity.Create(
            UserId.Of(Guid.Parse(existingMerchantUser.Id)),
            newMerchant.Name,
            newMerchant.Description,
            newMerchant.Address,
            newMerchant.PhoneNumber,
            newMerchant.Email);

        merchantEntity.Id = newMerchant.Id;

        merchantDbContext.Merchants.Add(merchantEntity);
        await merchantDbContext.SaveChangesAsync();
        existingMerchant = merchantEntity;
        Console.WriteLine($"   ✅ Merchant entity created: {merchantEntity.Name} ({merchantEntity.Email})");
    }
    else
    {
        Console.WriteLine($"   ⚠️  Merchant entity already exists: {existingMerchant.Name}");
    }

    // Create 10 catalogs for the merchant user
    Console.WriteLine("\n📦 Creating 10 catalog(s) for merchant user...");
    
    // Check if merchant already has catalogs
    List<CatalogEntity> existingCatalogs = await catalogDbContext.Catalogs
        .Where(c => c.MerchantId == existingMerchant.Id.Value)
        .ToListAsync();
    
    int merchantCatalogsCreated = existingCatalogs.Count;
    
    if (merchantCatalogsCreated >= 10)
    {
        Console.WriteLine($"   ⚠️  Merchant already has {merchantCatalogsCreated} catalog(s). Skipping creation.");
    }
    else
    {
        int catalogsToCreate = 10 - merchantCatalogsCreated;
        Console.WriteLine($"   Creating {catalogsToCreate} additional catalog(s) (already has {merchantCatalogsCreated})...");
        
        for (int j = 0; j < catalogsToCreate; j++)
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
                    merchantId: existingMerchant.Id.Value,
                    categories: catalog.Categories,
                    price: catalog.Price,
                    stock: catalog.Stock
                );

                catalogEntity.Id = CatalogId.Of(Guid.CreateVersion7());
                catalogDbContext.Catalogs.Add(catalogEntity);
                await catalogDbContext.SaveChangesAsync();
                merchantCatalogsCreated++;
                Console.WriteLine($"   ✅ {catalog.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error creating catalog: {ex.Message}");
            }
        }
    }

    // Create shopping cart for customer user
    Console.WriteLine("\n🛒 Creating shopping cart for customer user...");
    ShoppingCartEntity? existingCustomerCart = await basketDbContext.ShoppingCarts
        .FirstOrDefaultAsync(sc => sc.Username == "customer@marketspace.com");
    
    if (existingCustomerCart == null)
    {
        ShoppingCartEntity customerCart = BasketBuilder.CreateShoppingCartFaker(username: "customer@marketspace.com");
        basketDbContext.ShoppingCarts.Add(customerCart);
        await basketDbContext.SaveChangesAsync();
        Console.WriteLine($"   ✅ Cart created for customer with {customerCart.Items.Count} items");
    }
    else
    {
        Console.WriteLine($"   ⚠️  Shopping cart already exists for customer with {existingCustomerCart.Items.Count} items");
    }

    // Seed additional merchants and catalogs
    Console.WriteLine($"\n👨‍💼 Creating {merchantCount} additional merchant(s)...");
    List<MerchantEntity> createdMerchants = [];

    if (!await roleManager.RoleExistsAsync("Member"))
        await roleManager.CreateAsync(new IdentityRole("Member"));

    for (int i = 0; i < merchantCount; i++)
    {
        ApplicationUser user = UserBuilder.CreateApplicationUser();
        user.UserType = UserTypeEnum.Merchant;
        user.Name = user.Email;

        IdentityResult userResult = await userManager.CreateAsync(user, "Password123!");
        if (!userResult.Succeeded)
        {
            Console.WriteLine($"   ❌ Failed to create user {user.Email}: {string.Join(", ", userResult.Errors.Select(e => e.Description))}");
            continue;
        }

        await userManager.AddToRoleAsync(user, "Member");

        MerchantEntity merchant = MerchantBuilder.CreateMerchantFaker().Generate();
        merchant.CreatedBy = "seed";

        MerchantEntity merchantEntity = MerchantEntity.Create(
            UserId.Of(Guid.Parse(user.Id)),
            merchant.Name,
            merchant.Description,
            merchant.Address,
            merchant.PhoneNumber,
            merchant.Email);

        merchantEntity.Id = merchant.Id;

        merchantDbContext.Merchants.Add(merchantEntity);
        await merchantDbContext.SaveChangesAsync();
        createdMerchants.Add(merchantEntity);
        Console.WriteLine($"   ✅ {merchant.Name} ({merchant.Email})");
    }

    // Seed catalogs for additional merchants
    Console.WriteLine($"\n📦 Creating {catalogsPerMerchant} catalog(s) per additional merchant...");
    int totalCatalogsCreated = merchantCatalogsCreated;
    
    foreach (MerchantEntity createdMerchant in createdMerchants)
    {
        for (int j = 0; j < catalogsPerMerchant; j++)
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
                    merchantId: createdMerchant.Id.Value,
                    categories: catalog.Categories,
                    price: catalog.Price,
                    stock: catalog.Stock
                );

                catalogEntity.Id = CatalogId.Of(Guid.CreateVersion7());
                catalogDbContext.Catalogs.Add(catalogEntity);
                await catalogDbContext.SaveChangesAsync();
                totalCatalogsCreated++;
                Console.WriteLine($"   ✅ {catalog.Name} for {createdMerchant.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error creating catalog: {ex.Message}");
            }
        }
    }

    // Seed shopping carts for additional merchants
    Console.WriteLine("\n🛒 Creating shopping carts for additional merchants...");
    List<ShoppingCartEntity> createdShoppingCarts = [];
    
    foreach (MerchantEntity createdMerchant in createdMerchants)
    {
        ShoppingCartEntity shoppingCart = BasketBuilder.CreateShoppingCartFaker(username: createdMerchant.Name);
        createdShoppingCarts.Add(shoppingCart);
        basketDbContext.ShoppingCarts.Add(shoppingCart);
        Console.WriteLine($"   ✅ Cart for {createdMerchant.Name} with {shoppingCart.Items.Count} items");
    }
    await basketDbContext.SaveChangesAsync();

    Console.WriteLine("\n✅ Seed Mode completed.");
    Console.WriteLine($"   Created 1 merchant user (merchant@marketspace.com)");
    Console.WriteLine($"   Created 1 customer user (customer@marketspace.com)");
    Console.WriteLine($"   Created {merchantCatalogsCreated} catalog(s) for merchant user");
    Console.WriteLine($"   Created {createdMerchants.Count} additional merchant(s)");
    Console.WriteLine($"   Created {totalCatalogsCreated} total catalog(s)");
    Console.WriteLine($"   Created {createdShoppingCarts.Count + (existingCustomerCart == null ? 1 : 0)} shopping cart(s)");
}

// ── MODE 1 – Direct database access ────────────────────────────────────────
async Task RunDirectDbModeAsync(int recordsToCreate)
{
    Console.WriteLine("===========================================");
    Console.WriteLine(" MODO 1 – Acesso Direto ao Banco");
    Console.WriteLine("===========================================");

    Faker faker = new("pt_BR");

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
    UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    IMinioBucket minioBucket = scope.ServiceProvider.GetRequiredService<IMinioBucket>();

    Console.WriteLine("Creating schemas...");
    await merchantDbContext.Database.EnsureCreatedAsync();
    await catalogDbContext.Database.EnsureCreatedAsync();
    await basketDbContext.Database.EnsureCreatedAsync();
    await userDbContext.Database.EnsureCreatedAsync();
    Console.WriteLine("Schemas OK.");

    if (!await roleManager.RoleExistsAsync("Member"))
        await roleManager.CreateAsync(new IdentityRole("Member"));

    for (int i = 0; i < recordsToCreate; i++)
    {
        // Diversified user data
        string name = faker.Name.FullName();
        string uniqueUsername = faker.Internet.UserName(name) + faker.Random.Number(1000, 9999);
        string uniqueEmail = faker.Internet.Email(name);

        // Target user
        ApplicationUser? user = await userManager.FindByEmailAsync(uniqueEmail);

        if (user is null)
        {
            Console.WriteLine("Creating default user...");
            user = new ApplicationUser
            {
                UserName = uniqueUsername,
                Email = uniqueEmail,
                EmailConfirmed = true,
                Name = name,
                UserType = UserTypeEnum.Customer
            };
            
            IdentityResult userResult = await userManager.CreateAsync(user, "Password123!");
            if (!userResult.Succeeded)
            {
                Console.WriteLine($"   ❌ Failed to create user {user.Email}: {string.Join(", ", userResult.Errors.Select(e => e.Description))}");
                continue;
            }

            await userManager.AddToRoleAsync(user, "Member");
        }
        else
        {
            // Always update name to vary
            user.Name = faker.Name.FullName();
            await userDbContext.SaveChangesAsync();
        }

        Console.WriteLine($"✅ User: {user.Name} ({user.Email})");

        // Cart
        ShoppingCartEntity? cart = await basketDbContext.ShoppingCarts
            .FirstOrDefaultAsync(sc => sc.Username == uniqueUsername);

        // Diversified catalog items
        List<CatalogEntity> catalogItems = await catalogDbContext.Catalogs
            .OrderBy(_ => Guid.NewGuid())
            .Take(faker.Random.Int(1, 5))
            .ToListAsync();

        if (cart is null)
        {
            if (catalogItems.Count == 0)
            {
                Console.WriteLine("⚠️  No items in catalog. Cannot create cart.");
                continue;
            }

            cart = new ShoppingCartEntity
            {
                Username = uniqueUsername,
                Items = catalogItems.Select(c => new ShoppingCartItemEntity
                {
                    ProductId = c.Id.Value.ToString(),
                    ProductName = faker.Commerce.ProductName(),
                    Price = faker.Random.Decimal(10, 1000),
                    Quantity = faker.Random.Int(1, 4)
                }).ToList()
            };
            basketDbContext.ShoppingCarts.Add(cart);
            await basketDbContext.SaveChangesAsync();
            Console.WriteLine($"✅ Cart created with {cart.Items.Count} item(s).");
        }
        else
        {
            // Vary quantities
            foreach (ShoppingCartItemEntity item in cart.Items)
            {
                item.Quantity = faker.Random.Int(1, 4);
                item.ProductName = faker.Commerce.ProductName();
                item.Price = faker.Random.Decimal(10, 1000);
            }

            // Add new items randomly
            foreach (CatalogEntity newItem in catalogItems.Where(c =>
                         cart.Items.All(i => i.ProductId != c.Id.Value.ToString())))
            {
                cart.Items.Add(new ShoppingCartItemEntity
                {
                    ProductId = newItem.Id.Value.ToString(),
                    ProductName = faker.Commerce.ProductName(),
                    Price = faker.Random.Decimal(10, 1000),
                    Quantity = faker.Random.Int(1, 3)
                });
            }

            await basketDbContext.SaveChangesAsync();
            Console.WriteLine($"✅ Cart updated with {cart.Items.Count} item(s).");
        }

        PrintCartSummary(cart);

        // Randomize address/provider for checkout
        var checkoutAddress = new
        {
            AddressLine = faker.Address.StreetAddress(),
            Country = faker.Address.Country(),
            State = faker.Address.State(),
            ZipCode = faker.Address.ZipCode(),
            CardName = $"{user.Name}",
            CardNumber = faker.Finance.CreditCardNumber(),
            Expiration = faker.Date.Future(2).ToString("MM/yy"),
            Cvv = faker.Finance.CreditCardCvv(),
            PaymentMethod = faker.PickRandom(new[] { 1, 2, 3 }),
            Coordinates = $"{faker.Address.Latitude()},{faker.Address.Longitude()}"
        };

        Console.WriteLine("\n🚀 Performing checkout via Basket API...");
        await DoCheckoutAsync(cart, user, faker, basketApiBaseUrl, checkoutAddress);
    }

    Console.WriteLine("\n✅ Direct Database Mode completed.");
}

// ── MODE 2 – Real calls to BFF ─────────────────────────────────────────
async Task RunBffHttpModeAsync()
{
    Console.WriteLine("===========================================");
    Console.WriteLine(" MODO 2 – Via BFF HTTP");
    Console.WriteLine("===========================================");

    Faker faker = new("pt_BR");

    // Get a customer user from database
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
    UserDbContext userDbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();

    // Find a customer user
    ApplicationUser? customerUser = await userDbContext.Users
        .FirstOrDefaultAsync(u => u.UserType == UserTypeEnum.Customer);

    if (customerUser == null)
    {
        Console.WriteLine("⚠️  No customer user found in database. Please run seed mode first or create a customer user.");
        return;
    }

    Console.WriteLine($"📋 Using customer: {customerUser.Email} (ID: {customerUser.Id})");

    using HttpClient http = new(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
    http.BaseAddress = new Uri(bffBaseUrl);
    http.DefaultRequestHeaders.Add("Accept", "application/json");

    // 1. Login / get token
    Console.WriteLine("\n🔐 Autenticando no BFF...");
    string? token = await BffLoginAsync(http, customerUser.Email!, "123456");
    if (token is not null)
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    // 2. List catalog products
    Console.WriteLine("\n📦 Buscando catálogo via BFF...");
    List<BffCatalogItem> products = await BffGetCatalogAsync(http);

    if (products.Count == 0)
    {
        Console.WriteLine("⚠️  Nenhum produto retornado pelo BFF. Verifique se a aplicação está rodando.");
        return;
    }

    Console.WriteLine($"   {products.Count} produto(s) encontrado(s).");

    // 3. Add items to cart
    string username = customerUser.UserName ?? customerUser.Email!;
    int itemsToAdd = faker.Random.Int(1, Math.Min(products.Count, 4));
    List<BffCatalogItem> selectedItems = faker.PickRandom(products, itemsToAdd).ToList();

    Console.WriteLine($"\n🛒 Adicionando {selectedItems.Count} item(s) ao carrinho...");
    foreach (BffCatalogItem item in selectedItems)
    {
        int qty = faker.Random.Int(1, 3);
        bool added = await BffAddToBasketAsync(http, username, item, qty);
        Console.WriteLine(added
            ? $"   ✅ {item.Name} (x{qty}) @ R${item.Price:F2}"
            : $"   ❌ Falha ao adicionar {item.Name}");
    }

    // 4. Checkout
    Console.WriteLine("\n🚀 Realizando checkout via BFF...");
    bool checkoutOk = await BffCheckoutAsync(http, username, customerUser.Email!, faker);
    Console.WriteLine(checkoutOk
        ? "✅ Checkout realizado com sucesso!"
        : "❌ Checkout falhou. Verifique os logs da aplicação.");

    Console.WriteLine("\n✅ BFF HTTP Mode completed.");
}

// ── Helpers ─────────────────────────────────────────────────────────────────

static void PrintCartSummary(ShoppingCartEntity cart)
{
    Console.WriteLine("\n📦 Resumo do carrinho:");
    Console.WriteLine($"   Usuário : {cart.Username}");
    Console.WriteLine($"   Itens   : {cart.Items.Count}");
    foreach (ShoppingCartItemEntity item in cart.Items)
        Console.WriteLine($"     - {item.ProductName} (x{item.Quantity}) @ R${item.Price:F2}");
    Console.WriteLine($"   Total   : R${cart.TotalPrice:F2}");
}

static async Task DoCheckoutAsync(
    ShoppingCartEntity cart,
    ApplicationUser user,
    Faker faker,
    string baseUrl,
    dynamic checkoutAddress)
{
    try
    {
        using HttpClient http = new();
        http.BaseAddress = new Uri(baseUrl);
        http.DefaultRequestHeaders.Add("Accept", "application/json");

        var payload = new
        {
            UserName = cart.Username,
            CustomerId = user.Id,
            TotalPrice = cart.TotalPrice,
            FirstName = user.Name ?? faker.Person.FullName,
            EmailAddress = user.Email,
            AddressLine = checkoutAddress.AddressLine,
            Country = checkoutAddress.Country,
            State = checkoutAddress.State,
            ZipCode = checkoutAddress.ZipCode,
            CardName = checkoutAddress.CardName,
            CardNumber = checkoutAddress.CardNumber,
            Expiration = checkoutAddress.Expiration,
            Cvv = checkoutAddress.Cvv,
            PaymentMethod = checkoutAddress.PaymentMethod,
            RequestId = Guid.NewGuid().ToString(),
            Coordinates = checkoutAddress.Coordinates
        };

        HttpResponseMessage response = await http.PostAsJsonAsync("/basket/checkout", payload);

        if (response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"   ✅ Checkout OK! Resposta: {body}");
        }
        else
        {
            string err = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"   ❌ Checkout falhou ({response.StatusCode}): {err}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ❌ Exceção: {ex.Message}");
        Console.WriteLine($"   Verifique se a Basket API está rodando em {baseUrl}");
    }
}

// ── BFF helpers ──────────────────────────────────────────────────────────────

/// <summary>POST /auth/login  →  retorna Bearer token (ajuste o endpoint conforme seu BFF)</summary>
static async Task<string?> BffLoginAsync(HttpClient http, string email, string password)
{
    try
    {
        var body = new { email, password };
        HttpResponseMessage res = await http.PostAsJsonAsync("/auth/login", body);

        if (!res.IsSuccessStatusCode)
        {
            Console.WriteLine($"   ⚠️  Login retornou {res.StatusCode}. Continuando sem token...");
            return null;
        }

        using JsonDocument doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        // Tenta as chaves mais comuns; ajuste se necessário
        if (doc.RootElement.TryGetProperty("token", out JsonElement t)) return t.GetString();
        if (doc.RootElement.TryGetProperty("accessToken", out JsonElement a)) return a.GetString();
        if (doc.RootElement.TryGetProperty("access_token", out JsonElement b)) return b.GetString();

        Console.WriteLine("   ⚠️  Token não encontrado na resposta de login.");
        return null;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠️  Falha no login: {ex.Message}");
        return null;
    }
}

/// <summary>GET /catalog  →  lista de produtos (ajuste o endpoint conforme seu BFF)</summary>
static async Task<List<BffCatalogItem>> BffGetCatalogAsync(HttpClient http)
{
    try
    {
        HttpResponseMessage res = await http.GetAsync("/catalog");
        if (!res.IsSuccessStatusCode)
        {
            Console.WriteLine($"   ⚠️  Catálogo retornou {res.StatusCode}");
            return [];
        }

        string json = await res.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(json);

        // Suporta tanto array direto quanto objeto com propriedade "items"/"data"/"products"
        JsonElement root = doc.RootElement;
        JsonElement arr = root.ValueKind == JsonValueKind.Array
            ? root
            : root.TryGetProperty("items", out JsonElement i1)
                ? i1
                : root.TryGetProperty("data", out JsonElement i2)
                    ? i2
                    : root.TryGetProperty("products", out JsonElement i3)
                        ? i3
                        : root;

        return arr.EnumerateArray().Select(e => new BffCatalogItem
        {
            Id = e.TryGetProperty("id", out JsonElement id) ? id.ToString() : Guid.NewGuid().ToString(),
            Name = e.TryGetProperty("name", out JsonElement nm) ? nm.GetString() ?? "?" : "?",
            Price = e.TryGetProperty("price", out JsonElement pr) ? pr.GetDecimal() : 0m
        }).ToList();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠️  Erro ao buscar catálogo: {ex.Message}");
        return [];
    }
}

/// <summary>POST /basket  →  adiciona item ao carrinho (ajuste o endpoint conforme seu BFF)</summary>
static async Task<bool> BffAddToBasketAsync(
    HttpClient http,
    string username,
    BffCatalogItem item,
    int quantity)
{
    try
    {
        var body = new
        {
            username,
            items = new[] { new { productId = item.Id, productName = item.Name, price = item.Price, quantity } }
        };

        HttpResponseMessage res = await http.PostAsJsonAsync("/basket", body);
        return res.IsSuccessStatusCode;
    }
    catch
    {
        return false;
    }
}

/// <summary>POST /basket/checkout  →  finaliza compra via BFF</summary>
static async Task<bool> BffCheckoutAsync(HttpClient http, string username, string email, Faker faker)
{
    try
    {
        var body = new
        {
            userName = username,
            firstName = faker.Person.FirstName,
            lastName = faker.Person.LastName,
            emailAddress = email,
            addressLine = faker.Address.StreetAddress(),
            country = faker.Address.Country(),
            state = faker.Address.State(),
            zipCode = faker.Address.ZipCode(),
            cardName = faker.Person.FullName,
            cardNumber = faker.Finance.CreditCardNumber(),
            expiration = faker.Date.Future().ToString("MM/yy"),
            cvv = faker.Random.Number(100, 999).ToString(),
            paymentMethod = faker.Random.Int(1, 3),
            requestId = Guid.NewGuid().ToString(),
            coordinates = $"{faker.Address.Latitude()},{faker.Address.Longitude()}"
        };

        HttpResponseMessage res = await http.PostAsJsonAsync("/basket/checkout", body);

        if (res.IsSuccessStatusCode)
            return true;

        string err = await res.Content.ReadAsStringAsync();
        Console.WriteLine($"   Detalhe: {err}");

        return false;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠️  Exceção no checkout: {ex.Message}");
        return false;
    }
}

// ── MODE 5 – Full end-to-end DEMO ────────────────────────────────────────────
async Task RunDemoModeAsync()
{
    const string demoMerchantEmail = "merchant@marketspace.com";
    const string demoCustomerEmail = "customer@marketspace.com";
    const string demoPassword = "123456";
    List<DemoCatalogItem> merchantProducts = [];

    Console.WriteLine();
    Console.WriteLine("╔══════════════════════════════════════════════════╗");
    Console.WriteLine("║         MarketSpace – Full DEMO Mode             ║");
    Console.WriteLine("╚══════════════════════════════════════════════════╝");
    Console.WriteLine();
    Console.WriteLine($"  BFF URL : {bffBaseUrl}");
    Console.WriteLine($"  Merchant: {demoMerchantEmail}  / password: {demoPassword}");
    Console.WriteLine($"  Customer: {demoCustomerEmail}  / password: {demoPassword}");
    Console.WriteLine();

    Faker faker = new("en");

    // ── 0. Ensure demo users exist (db-level setup) ───────────────────────
    Console.WriteLine("── Step 0: Ensuring demo users exist in database ─────────");
    await EnsureDemoUsersAsync();

    // ── 1. Merchant Session ───────────────────────────────────────────────
    Console.WriteLine();
    Console.WriteLine("── Step 1: Merchant Session ──────────────────────────────");

    using HttpClient merchantHttp = new(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    }) { BaseAddress = new Uri(bffBaseUrl) };
    merchantHttp.DefaultRequestHeaders.Add("Accept", "application/json");

    Console.WriteLine($"  🔐 Logging in as merchant ({demoMerchantEmail})...");
    string? merchantToken = await DemoBffLoginAsync(merchantHttp, demoMerchantEmail, demoPassword);
    if (merchantToken is null)
    {
        Console.WriteLine("  ❌ Merchant login failed. Make sure the BFF is running and seed mode has been run.");
        return;
    }
    merchantHttp.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", merchantToken);
    Console.WriteLine("  ✅ Merchant logged in.");

    // Get merchant profile
    Console.WriteLine("  📋 Fetching merchant profile...");
    DemoMerchantProfile? merchantProfile = await DemoGetMerchantMeAsync(merchantHttp);
    if (merchantProfile is null)
    {
        Console.WriteLine("  ⚠️  Could not retrieve merchant profile. Ensure seed mode has been run first.");
    }
    else
    {
        Console.WriteLine($"  ✅ Merchant: {merchantProfile.Name} (ID: {merchantProfile.Id})");

        // Show current products
        Console.WriteLine("  📦 Fetching merchant's current products...");
        merchantProducts = await DemoGetMerchantProductsAsync(merchantHttp, merchantProfile.Id);
        Console.WriteLine($"  📊 Currently {merchantProducts.Count} product(s) in catalog.");

        // Create products via BFF if fewer than 10
        int targetProducts = 10;
        int toCreate = Math.Max(0, targetProducts - merchantProducts.Count);
        if (toCreate > 0)
        {
            Console.WriteLine($"  ➕ Creating {toCreate} product(s) via BFF...");
            for (int i = 0; i < toCreate; i++)
            {
                DemoCreateProductRequest newProduct = new()
                {
                    Name = faker.Commerce.ProductName(),
                    Description = faker.Commerce.ProductDescription(),
                    ImageUrl = faker.Image.PicsumUrl(800, 600),
                    Price = Math.Round(faker.Random.Decimal(5, 500), 2),
                    Stock = faker.Random.Int(10, 100),
                    Categories = [faker.Commerce.Department(), faker.Commerce.Department()],
                    MerchantId = merchantProfile.Id
                };

                bool created = await DemoCreateProductAsync(merchantHttp, newProduct);
                Console.WriteLine(created
                    ? $"    ✅ Created: {newProduct.Name} @ ${newProduct.Price:F2} (stock: {newProduct.Stock})"
                    : $"    ❌ Failed to create: {newProduct.Name}");
            }

            // Refresh product list
            merchantProducts = await DemoGetMerchantProductsAsync(merchantHttp, merchantProfile.Id);
        }

        Console.WriteLine($"  📊 Merchant catalog now has {merchantProducts.Count} product(s):");
        foreach (DemoCatalogItem p in merchantProducts.Take(5))
            Console.WriteLine($"    • {p.Name,-35} ${p.Price,8:F2}   stock: {p.Stock}");
    }

    // ── 2. Customer Session ───────────────────────────────────────────────
    Console.WriteLine();
    Console.WriteLine("── Step 2: Customer Session ──────────────────────────────");

    using HttpClient customerHttp = new(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    }) { BaseAddress = new Uri(bffBaseUrl) };
    customerHttp.DefaultRequestHeaders.Add("Accept", "application/json");

    Console.WriteLine($"  🔐 Logging in as customer ({demoCustomerEmail})...");
    string? customerToken = await DemoBffLoginAsync(customerHttp, demoCustomerEmail, demoPassword);
    if (customerToken is null)
    {
        Console.WriteLine("  ❌ Customer login failed.");
        return;
    }
    customerHttp.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", customerToken);
    Console.WriteLine("  ✅ Customer logged in.");

    // Browse catalog
    Console.WriteLine("  🔍 Browsing catalog...");
    List<DemoCatalogItem> allProducts = await DemoGetCatalogAsync(customerHttp);
    Console.WriteLine($"  📦 Found {allProducts.Count} product(s) in catalog.");

    if (allProducts.Count == 0)
    {
        Console.WriteLine("  ⚠️  Catalog is empty. Cannot proceed with shopping.");
        return;
    }

    List<DemoCatalogItem> candidateProducts = merchantProducts.Count > 0 ? merchantProducts : allProducts;
    int itemsToPick = faker.Random.Int(1, Math.Min(3, candidateProducts.Count));
    List<DemoCatalogItem> chosenItems = faker.PickRandom(candidateProducts, itemsToPick).ToList();

    // Add to cart
    Console.WriteLine($"  🛒 Adding {chosenItems.Count} item(s) to cart...");
    string cartUsername = demoCustomerEmail;
    foreach (DemoCatalogItem item in chosenItems)
    {
        int qty = faker.Random.Int(1, 3);
        bool added = await DemoAddToCartAsync(customerHttp, cartUsername, item, qty);
        Console.WriteLine(added
            ? $"    ✅ Added: {item.Name} × {qty}  @ ${item.Price:F2}"
            : $"    ❌ Failed to add: {item.Name}");
    }

    // Checkout
    Console.WriteLine("  🚀 Checking out...");
    (bool checkoutOk, string checkoutDetail) = await DemoCheckoutAsync(customerHttp, cartUsername, demoCustomerEmail, faker);
    if (checkoutOk)
    {
        Console.WriteLine("  ✅ Checkout successful! Order has been placed.");
    }
    else
    {
        Console.WriteLine($"  ❌ Checkout failed: {checkoutDetail}");
    }

    // ── 3. Summary ────────────────────────────────────────────────────────
    Console.WriteLine();
    Console.WriteLine("╔══════════════════════════════════════════════════╗");
    Console.WriteLine("║                  DEMO Complete                  ║");
    Console.WriteLine("╠══════════════════════════════════════════════════╣");
    Console.WriteLine($"║  Merchant products: {(merchantProfile is null ? "n/a" : "≥5 in catalog"),28} ║");
    Console.WriteLine($"║  Items added to cart : {chosenItems.Count,-27} ║");
    Console.WriteLine($"║  Checkout            : {(checkoutOk ? "✅ success" : "❌ failed"),-27} ║");
    Console.WriteLine("╚══════════════════════════════════════════════════╝");
}

// ── DEMO helpers ─────────────────────────────────────────────────────────────

async Task EnsureDemoUsersAsync()
{
    const string demoMerchantEmail = "merchant@marketspace.com";
    const string demoCustomerEmail = "customer@marketspace.com";

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
    UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    UserDbContext userDbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    MerchantDbContext merchantDbContext = scope.ServiceProvider.GetRequiredService<MerchantDbContext>();
    await userDbContext.Database.EnsureCreatedAsync();
    await merchantDbContext.Database.EnsureCreatedAsync();

    if (!await roleManager.RoleExistsAsync("Member"))
        await roleManager.CreateAsync(new IdentityRole("Member"));

    async Task<ApplicationUser?> EnsureUser(string email, UserTypeEnum type, string displayName)
    {
        ApplicationUser? existing = await userManager.FindByEmailAsync(email);
        if (existing is null)
        {
            existing = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Name = displayName,
                UserType = type
            };
            IdentityResult result = await userManager.CreateAsync(existing, "123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(existing, "Member");
                Console.WriteLine($"  ✅ Created user: {email}");
            }
            else
            {
                Console.WriteLine($"  ❌ Failed to create {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return null;
            }
        }
        else
        {
            Console.WriteLine($"  ✔  User already exists: {email}");
        }
        return existing;
    }

    ApplicationUser? merchantUser = await EnsureUser(demoMerchantEmail, UserTypeEnum.Merchant, "Demo Merchant");
    await EnsureUser(demoCustomerEmail, UserTypeEnum.Customer, "Demo Customer");

    // Ensure a MerchantEntity exists in MerchantDb for the demo merchant user
    if (merchantUser is not null && Guid.TryParse(merchantUser.Id, out Guid merchantUserId))
    {
        UserId userId = UserId.Of(merchantUserId);
        MerchantEntity? demoMerchant = await merchantDbContext.Merchants
            .FirstOrDefaultAsync(m => m.UserId == userId);

        if (demoMerchant is null)
        {
            demoMerchant = MerchantEntity.Create(
                userId: userId,
                name: "MarketSpace Demo Merchant",
                description: "Demo merchant account used to showcase the merchant dashboard and sales flow.",
                address: "123 Demo Street, Demo City",
                phoneNumber: "+1-555-0100",
                email: Merchant.Api.Domain.ValueObjects.Email.Of(demoMerchantEmail)
            );
            merchantDbContext.Merchants.Add(demoMerchant);
            Console.WriteLine($"  ✅ Created merchant entity for: {demoMerchantEmail}");
        }
        else
        {
            demoMerchant.Update(
                name: "MarketSpace Demo Merchant",
                description: "Demo merchant account used to showcase the merchant dashboard and sales flow.",
                address: "123 Demo Street, Demo City",
                phoneNumber: "+1-555-0100",
                email: Merchant.Api.Domain.ValueObjects.Email.Of(demoMerchantEmail));
            Console.WriteLine($"  ✔  Merchant entity refreshed for: {demoMerchantEmail}");
        }

        await merchantDbContext.SaveChangesAsync();
    }
}

static async Task<string?> DemoBffLoginAsync(HttpClient http, string email, string password)
{
    try
    {
        var body = new { Email = email, Password = password };
        HttpResponseMessage res = await http.PostAsJsonAsync("/api/auth/login", body);
        string responseBody = await res.Content.ReadAsStringAsync();

        if (!res.IsSuccessStatusCode)
        {
            Console.WriteLine($"  ⚠️  Login returned HTTP {(int)res.StatusCode}: {responseBody}");
            return null;
        }

        using JsonDocument doc = JsonDocument.Parse(responseBody);
        if (doc.RootElement.TryGetProperty("accessToken", out JsonElement a)) return a.GetString();
        if (doc.RootElement.TryGetProperty("token", out JsonElement t)) return t.GetString();

        Console.WriteLine($"  ⚠️  No token found in response: {responseBody}");
        return null;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ⚠️  Login exception: {ex.GetType().Name}: {ex.Message}");
        return null;
    }
}

static async Task<DemoMerchantProfile?> DemoGetMerchantMeAsync(HttpClient http)
{
    try
    {
        HttpResponseMessage res = await http.GetAsync("/api/merchant/me");
        if (!res.IsSuccessStatusCode) return null;
        using JsonDocument doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        JsonElement root = doc.RootElement.TryGetProperty("data", out JsonElement d) ? d : doc.RootElement;
        return new DemoMerchantProfile
        {
            Id = root.TryGetProperty("id", out JsonElement id) ? id.GetGuid() : Guid.Empty,
            Name = root.TryGetProperty("name", out JsonElement nm) ? nm.GetString() ?? "" : ""
        };
    }
    catch { return null; }
}

static async Task<List<DemoCatalogItem>> DemoGetMerchantProductsAsync(HttpClient http, Guid merchantId)
{
    try
    {
        HttpResponseMessage res = await http.GetAsync($"/api/catalog/merchant/{merchantId}?PageIndex=1&PageSize=50");
        if (!res.IsSuccessStatusCode) return [];
        return ParseCatalogItems(await res.Content.ReadAsStringAsync());
    }
    catch { return []; }
}

static async Task<List<DemoCatalogItem>> DemoGetCatalogAsync(HttpClient http)
{
    try
    {
        HttpResponseMessage res = await http.GetAsync("/api/catalog?PageIndex=1&PageSize=50");
        if (!res.IsSuccessStatusCode) return [];
        return ParseCatalogItems(await res.Content.ReadAsStringAsync());
    }
    catch { return []; }
}

static List<DemoCatalogItem> ParseCatalogItems(string json)
{
    using JsonDocument doc = JsonDocument.Parse(json);
    JsonElement root = doc.RootElement;

    // Unwrap Result<T> envelope if present
    JsonElement data = root.TryGetProperty("data", out JsonElement d) ? d : root;

    JsonElement arr = data.ValueKind == JsonValueKind.Array
        ? data
        : data.TryGetProperty("products", out JsonElement p) ? p
        : data.TryGetProperty("items", out JsonElement i) ? i
        : data;

    if (arr.ValueKind != JsonValueKind.Array) return [];

    return arr.EnumerateArray().Select(e => new DemoCatalogItem
    {
        Id = e.TryGetProperty("id", out JsonElement id) ? id.ToString() : "",
        Name = e.TryGetProperty("name", out JsonElement nm) ? nm.GetString() ?? "" : "",
        Price = e.TryGetProperty("price", out JsonElement pr) ? pr.GetDecimal() : 0m,
        Stock = e.TryGetProperty("stock", out JsonElement st) ? st.GetInt32() : 0
    }).Where(i => !string.IsNullOrEmpty(i.Id)).ToList();
}

static async Task<bool> DemoCreateProductAsync(HttpClient http, DemoCreateProductRequest product)
{
    try
    {
        var body = new
        {
            name = product.Name,
            description = product.Description,
            imageUrl = product.ImageUrl,
            price = product.Price,
            stock = product.Stock,
            categories = product.Categories,
            merchantId = product.MerchantId
        };
        HttpResponseMessage res = await http.PostAsJsonAsync("/api/catalog", body);
        return res.IsSuccessStatusCode;
    }
    catch { return false; }
}

static async Task<bool> DemoAddToCartAsync(HttpClient http, string username, DemoCatalogItem item, int quantity)
{
    try
    {
        var body = new
        {
            username,
            items = new[] { new { productId = item.Id, productName = item.Name, price = item.Price, quantity } }
        };
        HttpResponseMessage res = await http.PostAsJsonAsync("/api/basket", body);
        return res.IsSuccessStatusCode;
    }
    catch { return false; }
}

static async Task<(bool ok, string detail)> DemoCheckoutAsync(
    HttpClient http, string username, string email, Faker faker)
{
    try
    {
        var body = new
        {
            userName = username,
            firstName = faker.Person.FirstName,
            lastName = faker.Person.LastName,
            emailAddress = email,
            addressLine = faker.Address.StreetAddress(),
            country = faker.Address.Country(),
            state = faker.Address.State(),
            zipCode = faker.Address.ZipCode(),
            cardName = faker.Person.FullName,
            cardNumber = faker.Finance.CreditCardNumber(),
            expiration = faker.Date.Future().ToString("MM/yy"),
            cvv = faker.Random.Number(100, 999).ToString(),
            paymentMethod = faker.Random.Int(1, 3),
            requestId = Guid.NewGuid().ToString()
        };

        HttpResponseMessage res = await http.PostAsJsonAsync("/api/basket/checkout", body);
        if (res.IsSuccessStatusCode) return (true, string.Empty);

        string err = await res.Content.ReadAsStringAsync();
        return (false, $"HTTP {(int)res.StatusCode}: {err}");
    }
    catch (Exception ex)
    {
        return (false, ex.Message);
    }
}

// ── DEMO DTOs ─────────────────────────────────────────────────────────────────

internal sealed class DemoMerchantProfile
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

internal sealed class DemoCatalogItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

internal sealed class DemoCreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public List<string> Categories { get; set; } = [];
    public Guid MerchantId { get; set; }
}



internal sealed record BffCatalogItem
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
}

internal sealed record MerchantRawDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
