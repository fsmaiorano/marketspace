using BuildingBlocks;
using BuildingBlocks.Loggers.Abstractions;
using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Application.Merchant.CreateMerchant;

public sealed class CreateMerchantHandler(
    IMerchantRepository repository, 
    IApplicationLogger<CreateMerchantHandler> applicationLogger,
    IBusinessLogger<CreateMerchantHandler> businessLogger)
    : ICreateMerchantHandler
{
    public async Task<Result<CreateMerchantResult>> HandleAsync(CreateMerchantCommand command)
    {
        try
        {
            applicationLogger.LogInformation("Processing create merchant request for: {Name}", command.Name);
            
            MerchantEntity merchantEntity = MerchantEntity.Create(
                command.Name,
                command.Description,
                command.Address,
                command.PhoneNumber,
                Email.Of(command.Email));
            
            int result = await repository.AddAsync(merchantEntity);
            
            if (result <= 0)
            {
                applicationLogger.LogError("Failed to persist merchant to database for: {Name}", command.Name);
                return Result<CreateMerchantResult>.Failure("Failed to create merchant.");
            }
            
            businessLogger.LogInformation("Merchant created successfully. MerchantId: {MerchantId}, Name: {Name}, Email: {Email}", 
                merchantEntity.Id, 
                command.Name, 
                command.Email);
            return Result<CreateMerchantResult>.Success(new CreateMerchantResult(merchantEntity.Id.Value));
        }
        catch (Exception ex)
        {
            applicationLogger.LogError(ex, "An error occurred while creating the merchant: {Command}", command);
            return Result<CreateMerchantResult>.Failure($"An error occurred while creating the merchant: {ex.Message}");
        }
    }
}