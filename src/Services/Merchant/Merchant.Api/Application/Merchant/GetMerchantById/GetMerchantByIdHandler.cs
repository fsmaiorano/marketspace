using BuildingBlocks;
using BuildingBlocks.Loggers;
using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Application.Merchant.GetMerchantById;

public class GetMerchantByIdHandler(
    IMerchantRepository repository, 
    IAppLogger<GetMerchantByIdHandler> logger)
    : IGetMerchantByIdHandler
{
    public async Task<Result<GetMerchantByIdResult>> HandleAsync(GetMerchantByIdQuery query)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing get merchant by ID request for: {MerchantId}", query.Id);
            
            MerchantId merchantId = MerchantId.Of(query.Id);

            MerchantEntity? merchant = await repository.GetByIdAsync(merchantId, isTrackingEnabled: false);

            if (merchant is null)
            {
                logger.LogInformation(LogTypeEnum.Application, "Merchant with ID {MerchantId} not found.", query.Id);
                return Result<GetMerchantByIdResult>.Failure($"Catalog with ID {query.Id} not found.");
            }

            GetMerchantByIdResult result = new GetMerchantByIdResult
            {
                Id = merchant.Id.Value,
                Name = merchant.Name,
                Email = merchant.Email.Value,
                PhoneNumber = merchant.PhoneNumber,
                Address = merchant.Address
            };

            return Result<GetMerchantByIdResult>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while getting merchant by ID.");
            return Result<GetMerchantByIdResult>.Failure("An error occurred while processing your request.");
        }
    }
}