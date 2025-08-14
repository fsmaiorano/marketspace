using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Domain.Repositories;

public interface IMerchantRepository
{
    Task<int> AddAsync(MerchantEntity merchant, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(MerchantEntity merchant, CancellationToken cancellationToken = default);
    Task<int> RemoveAsync(MerchantId id, CancellationToken cancellationToken = default);
    Task<MerchantEntity?> GetByIdAsync(MerchantId id, CancellationToken cancellationToken = default);
}