using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Application.Merchant.DeleteMerchant;

public class DeleteMerchantHandler(IMerchantRepository repository, ILogger<DeleteMerchantHandler> logger)
    : IDeleteMerchantHandler
{
    public async Task<Result<DeleteMerchantResult>> HandleAsync(DeleteMerchantCommand command)
    {
        try
        {
            MerchantId merchantId = MerchantId.Of(command.Id);

            await repository.RemoveAsync(merchantId);
            logger.LogInformation("Catalog deleted successfully: {MerchantId}", command.Id);
            return Result<DeleteMerchantResult>.Success(new DeleteMerchantResult(true));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting the merchant: {Command}", command);
            return Result<DeleteMerchantResult>.Failure("An error occurred while deleting the merchant.");
        }
    }
}