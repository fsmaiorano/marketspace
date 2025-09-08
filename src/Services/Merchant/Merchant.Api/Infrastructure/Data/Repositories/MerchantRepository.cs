using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Merchant.Api.Infrastructure.Data.Repositories;

public class MerchantRepository(IMerchantDbContext dbContext) : IMerchantRepository
{
    public async Task<int> AddAsync(MerchantEntity merchant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(merchant, nameof(merchant));
        merchant.Id = MerchantId.Of(Guid.CreateVersion7());
        await dbContext.Merchants.AddAsync(merchant, cancellationToken);
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> UpdateAsync(MerchantEntity merchant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(merchant, nameof(merchant));

        MerchantEntity storedEntity = await GetByIdAsync(merchant.Id, cancellationToken: cancellationToken)
                                      ?? throw new InvalidOperationException(
                                          $"Catalog with ID {merchant.Id} not found.");

        storedEntity.Update(
            merchant.Name,
            merchant.Description,
            merchant.Address,
            merchant.PhoneNumber,
            merchant.Email);

        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> RemoveAsync(MerchantId id, CancellationToken cancellationToken = default)
    {
        MerchantEntity? storedEntity = await GetByIdAsync(id, cancellationToken: cancellationToken)
                                       ?? throw new InvalidOperationException(
                                           $"Catalog with ID {id} not found.");

        dbContext.Merchants.Remove(storedEntity);
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MerchantEntity?> GetByIdAsync(MerchantId id, bool isTrackingEnabled = true,
        CancellationToken cancellationToken = default)
    {
        if (isTrackingEnabled)
            return await dbContext.Merchants.FirstOrDefaultAsync(m => m.Id.Equals(id),
                cancellationToken: cancellationToken);

        return await dbContext.Merchants.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id.Equals(id), cancellationToken);
    }
}