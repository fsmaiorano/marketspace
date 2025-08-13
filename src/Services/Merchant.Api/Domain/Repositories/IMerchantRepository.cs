using Merchant.Api.Domain.Entities;

namespace Merchant.Api.Domain.Repositories;

public interface IMerchantRepository
{
    Task<int> AddAsync(MerchantEntity merchant);
    Task UpdateAsync(MerchantEntity merchant);
    Task DeleteAsync(MerchantEntity merchant);
    Task<MerchantEntity?> GetByIdAsync(Guid id);
}