using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Application.Merchant.UpdateMerchant;

public sealed class UpdateMerchantHandler(IMerchantRepository merchantRepository, ILogger<UpdateMerchantHandler> logger)
    : IUpdateMerchantHandler
{
    public async Task<Result<UpdateMerchantResult>> HandleAsync(UpdateMerchantCommand command)
    {
        try
        {
            MerchantEntity merchantEntity = MerchantEntity.Create(
                command.Name,
                command.Description,
                command.Address,
                command.PhoneNumber,
                Email.Of(command.Email));
            
            merchantEntity.Id = MerchantId.Of(command.Id);

            await merchantRepository.UpdateAsync(merchantEntity);
            logger.LogInformation("Merchant updated successfully: {MerchantId}", command.Id);

            return Result<UpdateMerchantResult>.Success(new UpdateMerchantResult(isSuccess: true));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating the merchant: {Command}", command);
            return Result<UpdateMerchantResult>.Failure("An error occurred while updating the merchant.");
        }
    }
}