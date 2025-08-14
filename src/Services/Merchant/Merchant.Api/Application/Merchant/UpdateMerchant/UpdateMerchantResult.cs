namespace Merchant.Api.Application.Merchant.UpdateMerchant;

public class UpdateMerchantResult()
{
    public bool IsSuccess { get; init; }

    public UpdateMerchantResult(bool isSuccess) : this()
    {
        this.IsSuccess = isSuccess;
    }
}