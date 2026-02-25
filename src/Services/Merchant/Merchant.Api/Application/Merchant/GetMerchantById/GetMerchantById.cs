using BuildingBlocks.Loggers;
using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Application.Merchant.GetMerchantById;

public record GetMerchantByIdQuery(Guid Id);

public record GetMerchantByIdResult(MerchantEntity Merchant);

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

            return merchant is not null
                ? Result<GetMerchantByIdResult>.Success(new GetMerchantByIdResult(merchant))
                : Result<GetMerchantByIdResult>.Failure($"Catalog with ID {query.Id} not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while getting merchant by ID.");
            return Result<GetMerchantByIdResult>.Failure("An error occurred while processing your request.");
        }
    }
}