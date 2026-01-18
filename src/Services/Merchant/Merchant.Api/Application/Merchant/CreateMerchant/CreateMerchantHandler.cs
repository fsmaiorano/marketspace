using BuildingBlocks;
using BuildingBlocks.Loggers;
using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Application.Merchant.CreateMerchant;

public sealed class CreateMerchantHandler(
    IMerchantRepository repository, 
    IAppLogger<CreateMerchantHandler> logger)
    : ICreateMerchantHandler
{
    public async Task<Result<CreateMerchantResult>> HandleAsync(CreateMerchantCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing create merchant request for: {Name}", command.Name);
            
            MerchantEntity merchantEntity = MerchantEntity.Create(
                command.Name,
                command.Description,
                command.Address,
                command.PhoneNumber,
                Email.Of(command.Email));
            
            int result = await repository.AddAsync(merchantEntity);
            
            if (result <= 0)
            {
                logger.LogError(LogTypeEnum.Application, null, "Failed to persist merchant to database for: {Name}", command.Name);
                return Result<CreateMerchantResult>.Failure("Failed to create merchant.");
            }
            
            logger.LogInformation(LogTypeEnum.Business, "Merchant created successfully. MerchantId: {MerchantId}, Name: {Name}, Email: {Email}", 
                merchantEntity.Id, 
                command.Name, 
                command.Email);
            return Result<CreateMerchantResult>.Success(new CreateMerchantResult(merchantEntity.Id.Value));
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while creating the merchant: {Command}", command);
            return Result<CreateMerchantResult>.Failure($"An error occurred while creating the merchant: {ex.Message}");
        }
    }
}