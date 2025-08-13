using BuildingBlocks;

namespace Merchant.Api.Application.Merchant.CreateMerchant;

public interface ICreateMerchantHandler
{
    Task<Result<CreateMerchantResult>> HandleAsync(CreateMerchantCommand command);
}