namespace Merchant.Api.Application.Merchant.CreateMerchant;

public class CreateMerchantResult()
{
    public Guid MerchantId { get; init; }

    public CreateMerchantResult(Guid merchantId) : this()
    {
        MerchantId = merchantId;
    }
}