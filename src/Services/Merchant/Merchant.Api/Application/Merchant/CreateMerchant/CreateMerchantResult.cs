namespace Merchant.Api.Application.Merchant.CreateMerchant;

public class CreateMerchantResult(Guid merchantId)
{
    public Guid MerchantId { get; init; } = merchantId;
}