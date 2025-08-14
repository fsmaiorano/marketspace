namespace Merchant.Api.Application.Merchant.GetMerchantById;

public interface IGetMerchantByIdHandler
{
    Task<Result<GetMerchantByIdResult>> HandleAsync(GetMerchantByIdQuery query);
}