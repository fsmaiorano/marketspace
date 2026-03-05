using BuildingBlocks.Loggers;
using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Application.Merchant.CreateMerchant;

public record CreateMerchantCommand()
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public record CreateMerchantResult();

public sealed class CreateMerchant(
    IMerchantRepository repository,
    IAppLogger<CreateMerchant> logger)
{
    public async Task<Result<CreateMerchantResult>> HandleAsync(CreateMerchantCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing create merchant request for: {Name}",
                command.Name);

            MerchantEntity merchantEntity = MerchantEntity.Create(
                UserId.Of(command.UserId),
                command.Name,
                command.Description,
                command.Address,
                command.PhoneNumber,
                Email.Of(command.Email));

            bool alreadyExists = await repository.ExistsAsync(command.UserId, command.Email);

            if (alreadyExists)
            {
                logger.LogWarning(LogTypeEnum.Business,
                    "Merchant creation failed due to existing merchant with same UserId or Email. UserId: {UserId}, Email: {Email}",
                    command.UserId, command.Email);
                return Result<CreateMerchantResult>.Failure("A merchant with the same UserId or Email already exists.");
            }

            int result = await repository.AddAsync(merchantEntity);

            if (result <= 0)
            {
                logger.LogError(LogTypeEnum.Application, null, "Failed to persist merchant to database for: {Name}",
                    command.Name);
                return Result<CreateMerchantResult>.Failure("Failed to create merchant.");
            }

            logger.LogInformation(LogTypeEnum.Business,
                "Merchant created successfully. MerchantId: {MerchantId}, Name: {Name}, Email: {Email}",
                merchantEntity.Id,
                command.Name,
                command.Email);

            return Result<CreateMerchantResult>.Success(new CreateMerchantResult());
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while creating the merchant: {Command}",
                command);
            return Result<CreateMerchantResult>.Failure($"An error occurred while creating the merchant: {ex.Message}");
        }
    }
}