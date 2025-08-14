using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Domain.Repositories;

public interface IMerchantRepository
{
    Task<int> AddAsync(MerchantEntity merchant);
    Task<int> UpdateAsync(MerchantEntity merchant);
    Task DeleteAsync(MerchantEntity merchant);
    Task<MerchantEntity?> GetByIdAsync(MerchantId id);
}