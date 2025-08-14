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
            MerchantEntity? storedMerchant = await merchantRepository.GetByIdAsync(command.Id);

            if (storedMerchant is null)
            {
                logger.LogWarning("Merchant not found: {MerchantId}", command.Id);
                return Result<UpdateMerchantResult>.Failure("Merchant not found.");
            }

            MerchantEntity merchantEntity = MerchantEntity.Update(
                command.Id,
                command.Name,
                command.Description,
                command.Address,
                command.PhoneNumber,
                Email.Of(command.Email));

            await merchantRepository.UpdateAsync(merchantEntity);
            logger.LogInformation("Merchant updated successfully: {MerchantId}", command.Id);

            return Result<UpdateMerchantResult>.Success(new UpdateMerchantResult(merchantEntity.Id.Value));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating the merchant: {Command}", command);
            return Result<UpdateMerchantResult>.Failure("An error occurred while updating the merchant.");
        }
    }
}