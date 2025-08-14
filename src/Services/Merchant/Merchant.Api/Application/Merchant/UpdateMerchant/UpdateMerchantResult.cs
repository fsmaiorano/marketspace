namespace Merchant.Api.Application.Merchant.UpdateMerchant;

public class UpdateMerchantResult()
{
    public Guid MerchantId { get; init; }

    public UpdateMerchantResult(Guid merchantId) : this()
    {
        MerchantId = merchantId;
    }
}