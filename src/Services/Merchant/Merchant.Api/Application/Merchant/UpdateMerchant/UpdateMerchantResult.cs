namespace Merchant.Api.Application.Merchant.UpdateMerchant;

public class UpdateMerchantResult(bool isSuccess)
{
    public bool IsSuccess { get; init; } = isSuccess;
}