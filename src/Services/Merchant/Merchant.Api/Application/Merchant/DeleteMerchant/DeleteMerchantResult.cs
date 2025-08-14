namespace Merchant.Api.Application.Merchant.DeleteMerchant;

public class DeleteMerchantResult(bool isSuccess)
{
    public bool IsSuccess { get; init; } = isSuccess;
}