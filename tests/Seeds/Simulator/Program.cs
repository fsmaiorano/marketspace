using Bogus;
using Merchant.Api.Domain.ValueObjects;
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
const string bffBaseUrl = "http://localhost:5000"; // ajuste para a porta do seu BFF
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
    ApplicationUser? existingMerchantUser = await userDbContext.Users
        .FirstOrDefaultAsync(u => u.UserName == "merchant@marketspace.com");
    
    if (existingMerchantUser == null)
    {
        existingMerchantUser = new ApplicationUser
        {
            UserName = "merchant@marketspace.com",
            Email = "merchant@marketspace.com",
            EmailConfirmed = true,
            PasswordHash = "123456",
            Name = "Merchant User",
        };
        await userDbContext.Users.AddAsync(existingMerchantUser);
        await userDbContext.SaveChangesAsync();
        Console.WriteLine($"✅ Merchant user created: {existingMerchantUser.Email} (ID: {existingMerchantUser.Id})");
    }
    else
    {
        Console.WriteLine($"⚠️  Merchant user already exists: {existingMerchantUser.Email} (ID: {existingMerchantUser.Id})");
    }

    // Check if customer user already exists
    ApplicationUser? existingCustomerUser = await userDbContext.Users
        .FirstOrDefaultAsync(u => u.UserName == "customer@marketspace.com");
    
    if (existingCustomerUser == null)
    {
        existingCustomerUser = new ApplicationUser
        {
            UserName = "customer@marketspace.com",
            Email = "customer@marketspace.com",
            EmailConfirmed = true,
            PasswordHash = "123456",
            Name = "Customer User",
        };
        await userDbContext.Users.AddAsync(existingCustomerUser);
        await userDbContext.SaveChangesAsync();
        Console.WriteLine($"✅ Customer user created: {existingCustomerUser.Email} (ID: {existingCustomerUser.Id})");
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

    for (int i = 0; i < merchantCount; i++)
    {
        ApplicationUser user = UserBuilder.CreateApplicationUser();

        await userDbContext.Users.AddAsync(user);
        await userDbContext.SaveChangesAsync();

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
    
    for (int i = 0; i < createdMerchants.Count; i++)
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
                    merchantId: createdMerchants.ElementAt(i).Id.Value,
                    categories: catalog.Categories,
                    price: catalog.Price,
                    stock: catalog.Stock
                );

                catalogEntity.Id = CatalogId.Of(Guid.CreateVersion7());
                catalogDbContext.Catalogs.Add(catalogEntity);
                await catalogDbContext.SaveChangesAsync();
                totalCatalogsCreated++;
                Console.WriteLine($"   ✅ {catalog.Name} for {createdMerchants.ElementAt(i).Name}");
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
    
    for (int i = 0; i < createdMerchants.Count; i++)
    {
        ShoppingCartEntity shoppingCart = BasketBuilder.CreateShoppingCartFaker(username: createdMerchants.ElementAt(i).Name);
        createdShoppingCarts.Add(shoppingCart);
        basketDbContext.ShoppingCarts.Add(shoppingCart);
        await basketDbContext.SaveChangesAsync();
        Console.WriteLine($"   ✅ Cart for {createdMerchants.ElementAt(i).Name} with {shoppingCart.Items.Count} items");
    }

    Console.WriteLine("\n✅ Seed Mode completed.");
    Console.WriteLine($"   Created 1 merchant user (merchant@marketspace.com)");
    Console.WriteLine($"   Created 1 customer user (customer@marketspace.com)");
    Console.WriteLine($"   Created {merchantCatalogsCreated} catalog(s) for merchant user");
    Console.WriteLine($"   Created {createdMerchants.Count} additional merchant(s)");
    Console.WriteLine($"   Created {totalCatalogsCreated} total catalog(s)");
    Console.WriteLine($"   Created {createdShoppingCarts.Count + (existingCustomerCart != null ? 1 : 1)} shopping cart(s)");
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
    IMinioBucket minioBucket = scope.ServiceProvider.GetRequiredService<IMinioBucket>();

    Console.WriteLine("Creating schemas...");
    await merchantDbContext.Database.EnsureCreatedAsync();
    await catalogDbContext.Database.EnsureCreatedAsync();
    await basketDbContext.Database.EnsureCreatedAsync();
    await userDbContext.Database.EnsureCreatedAsync();
    Console.WriteLine("Schemas OK.");

    for (int i = 0; i < recordsToCreate; i++)
    {
        // Diversified user data
        string name = faker.Name.FullName();
        string uniqueUsername = faker.Internet.UserName(name) + faker.Random.Number(1000, 9999);
        string uniqueEmail = faker.Internet.Email(name);

        // Target user
        ApplicationUser? user = await userDbContext.Users
            .FirstOrDefaultAsync(u => u.UserName == uniqueUsername);

        if (user is null)
        {
            Console.WriteLine("Creating default user...");
            user = new ApplicationUser
            {
                UserName = uniqueUsername,
                Email = uniqueEmail,
                EmailConfirmed = true,
                PasswordHash = "Password123!",
                Name = name,
            };
            await userDbContext.Users.AddAsync(user);
            await userDbContext.SaveChangesAsync();
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

    using HttpClient http = new();
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
            return res.IsSuccessStatusCode;

        string err = await res.Content.ReadAsStringAsync();
        Console.WriteLine($"   Detalhe: {err}");

        return res.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠️  Exceção no checkout: {ex.Message}");
        return false;
    }
}

// ── Internal DTOs ────────────────────────────────────────────────────────────

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
