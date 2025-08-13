using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Repositories;

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

    public Task UpdateAsync(MerchantEntity merchant)
    {
        ArgumentNullException.ThrowIfNull(merchant, nameof(merchant));
        merchant.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.Merchants.Update(merchant);
        return dbContext.SaveChangesAsync();
    }

    public Task DeleteAsync(MerchantEntity merchant)
    {
        ArgumentNullException.ThrowIfNull(merchant, nameof(merchant));
        dbContext.Merchants.Remove(merchant);
        return dbContext.SaveChangesAsync();
    }

    public async Task<MerchantEntity?> GetByIdAsync(Guid id)
    {
        return await dbContext.Merchants.FindAsync(id);
    }
}