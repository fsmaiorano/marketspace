using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Merchant.Api.Infrastructure.Data.Repositories;

public class MerchantRepository(IMerchantDbContext dbContext) : IMerchantRepository
{
    public async Task<int> AddAsync(MerchantEntity merchant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(merchant);
        merchant.Id = MerchantId.Of(Guid.CreateVersion7());
        await dbContext.Merchants.AddAsync(merchant, cancellationToken);
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> UpdateAsync(MerchantEntity merchant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(merchant);
        
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> RemoveAsync(MerchantId id, CancellationToken cancellationToken = default)
    {
        MerchantEntity storedEntity = await GetByIdAsync(id, isTrackingEnabled: true, cancellationToken)
            ?? throw new InvalidOperationException($"Merchant with ID {id} not found.");

        dbContext.Merchants.Remove(storedEntity);
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MerchantEntity?> GetByIdAsync(MerchantId id, bool isTrackingEnabled = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<MerchantEntity> query = dbContext.Merchants;

        if (!isTrackingEnabled)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(m => m.Id.Equals(id), cancellationToken);
    }
}