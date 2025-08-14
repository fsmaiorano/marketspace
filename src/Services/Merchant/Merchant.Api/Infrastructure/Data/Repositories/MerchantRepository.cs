using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;

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

        MerchantEntity storedEntity = await GetByIdAsync(merchant.Id, cancellationToken)
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
        MerchantEntity? storedEntity = await GetByIdAsync(id, cancellationToken)
                                       ?? throw new InvalidOperationException(
                                           $"Merchant with ID {id} not found.");

        dbContext.Merchants.Remove(storedEntity);
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MerchantEntity?> GetByIdAsync(MerchantId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Merchants.FindAsync([id], cancellationToken: cancellationToken);
    }
}