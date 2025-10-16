using BuildingBlocks;
using BuildingBlocks.Loggers.Abstractions;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Application.Merchant.DeleteMerchant;

public class DeleteMerchantHandler(
    IMerchantRepository repository, 
    IApplicationLogger<DeleteMerchantHandler> applicationLogger,
    IBusinessLogger<DeleteMerchantHandler> businessLogger)
    : IDeleteMerchantHandler
{
    public async Task<Result<DeleteMerchantResult>> HandleAsync(DeleteMerchantCommand command)
    {
        try
        {
            applicationLogger.LogInformation("Processing delete merchant request for: {MerchantId}", command.Id);
            
            MerchantId merchantId = MerchantId.Of(command.Id);

            await repository.RemoveAsync(merchantId);
            
            businessLogger.LogInformation("Merchant deleted successfully. MerchantId: {MerchantId}", command.Id);
            return Result<DeleteMerchantResult>.Success(new DeleteMerchantResult(true));
        }
        catch (Exception ex)
        {
            applicationLogger.LogError(ex, "An error occurred while deleting the merchant: {Command}", command);
            return Result<DeleteMerchantResult>.Failure("An error occurred while deleting the merchant.");
        }
    }
}