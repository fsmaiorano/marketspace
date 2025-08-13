using BuildingBlocks;
using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Repositories;

namespace Merchant.Api.Application.Merchant.CreateMerchant;

public class CreateMerchantHandler(IMerchantRepository repository, ILogger<CreateMerchantHandler> logger)
    : ICreateMerchantHandler
{
    public virtual async Task<Result<CreateMerchantResult>> HandleAsync(CreateMerchantCommand command)
    {
        try
        {
            MerchantEntity merchantEntity = MerchantEntity.Create(
                command.Name,
                command.Description,
                command.Address,
                command.PhoneNumber,
                command.Email);
            
            int result = await repository.AddAsync(merchantEntity);
            
            if (result <= 0)
            {
                logger.LogError("Failed to create merchant: {Command}", command);
                return Result<CreateMerchantResult>.Failure("Failed to create merchant.");
            }
            
            logger.LogInformation("Merchant created successfully: {MerchantId}", merchantEntity.Id);
            return Result<CreateMerchantResult>.Success(new CreateMerchantResult(merchantEntity.Id.Value));
        }
        catch (Exception ex)
        {
            return Result<CreateMerchantResult>.Failure($"An error occurred while creating the merchant: {ex.Message}");
        }
    }
}