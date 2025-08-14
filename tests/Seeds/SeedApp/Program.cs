using SeedApp;
using Merchant.Api.Infrastructure.Data;
using Builder;
using Merchant.Api.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

MarketSpaceSeedFactory factory = new();
IServiceScopeFactory scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

using IServiceScope scope = scopeFactory.CreateScope();
MerchantDbContext dbContext = scope.ServiceProvider.GetRequiredService<MerchantDbContext>();

const int createMerchantCounter = 100;

for (int i = 0; i < createMerchantCounter; i++)
{
    MerchantEntity merchant = MerchantBuilder.CreateMerchantFaker().Generate();
    merchant.CreatedAt = DateTime.UtcNow;
    merchant.CreatedBy = "seed";
    dbContext.Merchants.Add(merchant);
    Console.WriteLine("Merchant created: " + merchant.Name + " (" + merchant.Email + ")");
}

dbContext.SaveChanges();

