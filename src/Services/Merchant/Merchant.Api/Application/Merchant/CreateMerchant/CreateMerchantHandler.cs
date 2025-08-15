using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Application.Merchant.CreateMerchant;

public sealed class CreateMerchantHandler(IMerchantRepository repository, ILogger<CreateMerchantHandler> logger)
    : ICreateMerchantHandler
{
    public async Task<Result<CreateMerchantResult>> HandleAsync(CreateMerchantCommand command)
    {
        try
        {
            MerchantEntity merchantEntity = MerchantEntity.Create(
                command.Name,
                command.Description,
                command.Address,
                command.PhoneNumber,
                Email.Of(command.Email));
            
            int result = await repository.AddAsync(merchantEntity);
            
            if (result <= 0)
            {
                logger.LogError("Failed to create merchant: {Command}", command);
                return Result<CreateMerchantResult>.Failure("Failed to create merchant.");
            }
            
            logger.LogInformation("Catalog created successfully: {MerchantId}", merchantEntity.Id);
            return Result<CreateMerchantResult>.Success(new CreateMerchantResult(merchantEntity.Id.Value));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the merchant: {Command}", command);
            return Result<CreateMerchantResult>.Failure($"An error occurred while creating the merchant: {ex.Message}");
        }
    }
}