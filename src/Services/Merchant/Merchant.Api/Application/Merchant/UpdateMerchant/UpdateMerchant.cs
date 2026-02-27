using BuildingBlocks.Loggers;
using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Application.Merchant.UpdateMerchant;

public record UpdateMerchantCommand
{
    public required Guid Id { get; init; }
    public string? Name { get; init; } 
    public string? Description { get; init; } 
    public string? Address { get; init; } 
    public string? PhoneNumber { get; init; } 
    public string? Email { get; init; } 
}

public record UpdateMerchantResult();

public sealed class UpdateMerchant(
    IMerchantRepository repository, 
    IAppLogger<UpdateMerchant> logger)
{
    public async Task<Result<UpdateMerchantResult>> HandleAsync(UpdateMerchantCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing update merchant request for: {MerchantId}", command.Id);
            
            MerchantId merchantId = MerchantId.Of(command.Id);
            MerchantEntity? merchantEntity = await repository.GetByIdAsync(merchantId, isTrackingEnabled: true, CancellationToken.None);
            
            if (merchantEntity == null)
            {
                logger.LogWarning(LogTypeEnum.Application, "Merchant not found for update: {MerchantId}", command.Id);
                return Result<UpdateMerchantResult>.Failure($"Merchant with ID {command.Id} not found.");
            }
            
            // Usar método de domínio para atualizar
            merchantEntity.Update(
                command.Name,
                command.Description,
                command.Address,
                command.PhoneNumber,
                Email.Of(command.Email ?? string.Empty));

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