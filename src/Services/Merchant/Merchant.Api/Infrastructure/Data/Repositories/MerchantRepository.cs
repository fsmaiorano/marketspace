using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Infrastructure.Data.Repositories;

public class MerchantRepository(IMerchantDbContext dbContext) : IMerchantRepository
{
    public async Task<int> AddAsync(MerchantEntity merchant)
    {
        ArgumentNullException.ThrowIfNull(merchant, nameof(merchant));
        merchant.CreatedAt = DateTimeOffset.UtcNow;
        await dbContext.Merchants.AddAsync(merchant);
        return await dbContext.SaveChangesAsync();
    }

    public async Task<int> UpdateAsync(MerchantEntity merchant)
    {
        ArgumentNullException.ThrowIfNull(merchant, nameof(merchant));

        MerchantEntity storedEntity = await GetByIdAsync(merchant.Id)
                                      ?? throw new InvalidOperationException(
                                          $"Merchant with ID {merchant.Id} not found.");

        storedEntity.Name = merchant.Name;
        storedEntity.Description = merchant.Description;
        storedEntity.Address = merchant.Address;
        storedEntity.PhoneNumber = merchant.PhoneNumber;
        storedEntity.Email = merchant.Email;
        storedEntity.UpdatedAt = DateTimeOffset.UtcNow;

        return await dbContext.SaveChangesAsync();
    }

    public Task DeleteAsync(MerchantEntity merchant)
    {
        ArgumentNullException.ThrowIfNull(merchant, nameof(merchant));
        dbContext.Merchants.Remove(merchant);
        return dbContext.SaveChangesAsync();
    }

    public async Task<MerchantEntity?> GetByIdAsync(MerchantId id)
    {
        return await dbContext.Merchants.FindAsync(id);
    }
}