using BuildingBlocks.Loggers;
using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Application.Merchant.GetMerchantById;

public record GetMerchantByIdQuery(Guid Id);

public record GetMerchantByIdResult(
    Guid Id,
    string Name,
    string Description,
    string Email,
    string PhoneNumber,
    string Address,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public class GetMerchantById(
    IMerchantRepository repository,
    IAppLogger<GetMerchantById> logger)
{
    public async Task<Result<GetMerchantByIdResult>> HandleAsync(GetMerchantByIdQuery query)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing get merchant by ID request for: {MerchantId}",
                query.Id);

            MerchantId merchantId = MerchantId.Of(query.Id);

            MerchantEntity? merchant = await repository.GetByIdAsync(merchantId, isTrackingEnabled: false);

            if (merchant is null)
            {
                logger.LogInformation(LogTypeEnum.Application, "Merchant with ID {MerchantId} not found.", query.Id);
                return Result<GetMerchantByIdResult>.Failure($"Catalog with ID {query.Id} not found.");
            }

            GetMerchantByIdResult result = new GetMerchantByIdResult(
                merchant.Id.Value,
                merchant.Name,
                merchant.Description,
                merchant.Email.Value,
                merchant.PhoneNumber,
                merchant.Address,
                merchant.CreatedAt,
                merchant.UpdatedAt);

            return Result<GetMerchantByIdResult>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while getting merchant by ID.");
            return Result<GetMerchantByIdResult>.Failure("An error occurred while processing your request.");
        }
    }
}