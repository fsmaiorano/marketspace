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
        merchant.Id = MerchantId.Of(Guid.NewGuid());
        merchant.CreatedAt = DateTimeOffset.UtcNow;
        await dbContext.Merchants.AddAsync(merchant, cancellationToken);
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> UpdateAsync(MerchantEntity merchant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(merchant, nameof(merchant));

        MerchantEntity storedEntity = await GetByIdAsync(merchant.Id, cancellationToken: cancellationToken)
                                      ?? throw new InvalidOperationException(
                                          $"Merchant with ID {merchant.Id} not found.");

        storedEntity.Name = merchant.Name;
        storedEntity.Description = merchant.Description;
        storedEntity.Address = merchant.Address;
        storedEntity.PhoneNumber = merchant.PhoneNumber;
        storedEntity.Email = merchant.Email;
        storedEntity.UpdatedAt = DateTimeOffset.UtcNow;

        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> RemoveAsync(MerchantId id, CancellationToken cancellationToken = default)
    {
        MerchantEntity? storedEntity = await GetByIdAsync(id, cancellationToken: cancellationToken)
                                       ?? throw new InvalidOperationException(
                                           $"Merchant with ID {id} not found.");

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