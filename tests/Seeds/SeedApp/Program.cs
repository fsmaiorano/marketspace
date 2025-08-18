using Basket.Api.Domain.Entities;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.ValueObjects;
using Catalog.Api.Infrastructure.Data;
using MongoDB.Driver;

MarketSpaceSeedFactory factory = new();
IServiceScopeFactory scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

using IServiceScope scope = scopeFactory.CreateScope();
MerchantDbContext merchantDbContext = scope.ServiceProvider.GetRequiredService<MerchantDbContext>();
CatalogDbContext catalogDbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
IMongoClient basketMongoClient = scope.ServiceProvider.GetRequiredService<IMongoClient>();

const int createMerchantCounter = 10;

List<MerchantEntity> createdMerchants = [];
List<CatalogEntity> createdCatalogs = [];
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
}

// Create catalog
for (int i = 0; i < createdMerchants.Count; i++)
{
    CatalogEntity catalog = CatalogBuilder.CreateCatalogFaker().Generate();
    catalog.CreatedBy = "seed";

    CatalogEntity catalogEntity = CatalogEntity.Create(
        name: catalog.Name,
        description: catalog.Description,
        imageUrl: catalog.ImageUrl,
        merchantId: createdMerchants.ElementAt(i).Id.Value,
        categories: catalog.Categories,
        price: catalog.Price
    );

    catalogEntity.Id = CatalogId.Of(Guid.NewGuid());
    createdCatalogs.Add(catalogEntity);
    catalogDbContext.Catalogs.Add(catalogEntity);
}

// Create basket
IMongoDatabase basketDb = basketMongoClient.GetDatabase("BasketDb");
IMongoCollection<ShoppingCartEntity>
    shoppingCartCollection = basketDb.GetCollection<ShoppingCartEntity>("ShoppingCart");

for (int i = 0; i < createdMerchants.Count; i++)
{
    ShoppingCartEntity shoppingCart =
        Builder.BasketBuilder.CreateShoppingCartFaker(username: createdMerchants.ElementAt(i).Name);
    createdShoppingCarts.Add(shoppingCart);
}

merchantDbContext.SaveChanges();
catalogDbContext.SaveChanges();
shoppingCartCollection.InsertMany(createdShoppingCarts);