using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.ValueObjects;
using Catalog.Api.Infrastructure.Data;

MarketSpaceSeedFactory factory = new();
IServiceScopeFactory scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

using IServiceScope scope = scopeFactory.CreateScope();
MerchantDbContext merchantDbContext = scope.ServiceProvider.GetRequiredService<MerchantDbContext>();
CatalogDbContext catalogDbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

const int createMerchantCounter = 1000;

List<Guid> createdmerchantIds = [];

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
    createdmerchantIds.Add(merchantEntity.Id.Value);
}

// Create catalog
for (int i = 0; i < createdmerchantIds.Count; i++)
{
    CatalogEntity catalog = CatalogBuilder.CreateCatalogFaker().Generate();
    catalog.CreatedBy = "seed";

    CatalogEntity catalogEntity = CatalogEntity.Create(
        name: catalog.Name,
        description: catalog.Description,
        imageUrl: catalog.ImageUrl,
        merchantId: createdmerchantIds.ElementAt(i),
        categories: catalog.Categories,
        price: catalog.Price
    );

    catalogEntity.Id = CatalogId.Of(Guid.NewGuid());

    catalogDbContext.Catalogs.Add(catalogEntity);
}

merchantDbContext.SaveChanges();
catalogDbContext.SaveChanges();
