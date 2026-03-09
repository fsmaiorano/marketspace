using BuildingBlocks;
using BuildingBlocks.Loggers;
using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Repositories;

namespace Merchant.Api.Application.Merchant.GetMerchantByUserId;

public record GetMerchantByUserIdQuery(Guid UserId);

public record GetMerchantByUserIdResult(MerchantEntity Merchant);

public class GetMerchantByUserId(
    IMerchantRepository repository,
    IAppLogger<GetMerchantByUserId> logger)
{
    public async Task<Result<GetMerchantByUserIdResult>> HandleAsync(GetMerchantByUserIdQuery query)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application,
                "Processing get merchant by UserId request for: {UserId}", query.UserId);

            MerchantEntity? merchant = await repository.GetByUserIdAsync(query.UserId);

            return merchant is not null
                ? Result<GetMerchantByUserIdResult>.Success(new GetMerchantByUserIdResult(merchant))
                : Result<GetMerchantByUserIdResult>.Failure($"Merchant with UserId {query.UserId} not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while getting merchant by UserId.");
            return Result<GetMerchantByUserIdResult>.Failure("An error occurred while processing your request.");
        }
    }
}
