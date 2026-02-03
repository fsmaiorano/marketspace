using BuildingBlocks;
using BuildingBlocks.Loggers;
using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Application.Merchant.UpdateMerchant;

public sealed class UpdateMerchantHandler(
    IMerchantRepository repository, 
    IAppLogger<UpdateMerchantHandler> logger)
    : IUpdateMerchantHandler
{
    public async Task<Result<UpdateMerchantResult>> HandleAsync(UpdateMerchantCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing update merchant request for: {MerchantId}", command.Id);
            
            MerchantEntity merchantEntity = MerchantEntity.Create(
                command.Name,
                command.Description,
                command.Address,
                command.PhoneNumber,
                Email.Of(command.Email));
            
            merchantEntity.Id = MerchantId.Of(command.Id);

            await repository.UpdateAsync(merchantEntity);
            
            logger.LogInformation(LogTypeEnum.Business, "Merchant updated successfully. MerchantId: {MerchantId}, Name: {Name}", 
                command.Id, 
                command.Name);

            return Result<UpdateMerchantResult>.Success(new UpdateMerchantResult());
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while updating the merchant: {Command}", command);
            return Result<UpdateMerchantResult>.Failure("An error occurred while updating the merchant.");
        }
    }
}