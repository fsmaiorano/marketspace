namespace Merchant.Api.Application.Merchant.DeleteMerchant;

public interface IDeleteMerchantHandler
{
    Task<Result<DeleteMerchantResult>> HandleAsync(DeleteMerchantCommand command);
}