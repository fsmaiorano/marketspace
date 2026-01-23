using BuildingBlocks.Loggers;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Application.Merchant.DeleteMerchant;

public class DeleteMerchantHandler(
    IMerchantRepository repository,
    IAppLogger<DeleteMerchantHandler> logger)
    : IDeleteMerchantHandler
{
    public async Task<Result<DeleteMerchantResult>> HandleAsync(DeleteMerchantCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing delete merchant request for: {MerchantId}", command.Id);

            MerchantId merchantId = MerchantId.Of(command.Id);

            await repository.RemoveAsync(merchantId);

            logger.LogInformation(LogTypeEnum.Business, "Merchant deleted successfully. MerchantId: {MerchantId}", command.Id);
            return Result<DeleteMerchantResult>.Success(new DeleteMerchantResult(true));
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while deleting the merchant: {Command}", command);
            return Result<DeleteMerchantResult>.Failure("An error occurred while deleting the merchant.");
        }
    }
}