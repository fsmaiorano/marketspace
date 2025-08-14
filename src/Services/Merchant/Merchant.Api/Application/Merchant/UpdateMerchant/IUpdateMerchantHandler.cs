namespace Merchant.Api.Application.Merchant.UpdateMerchant;

public interface IUpdateMerchantHandler
{
    Task<Result<UpdateMerchantResult>> HandleAsync(UpdateMerchantCommand command);
}