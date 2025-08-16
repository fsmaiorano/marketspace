using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Application.Merchant.GetMerchantById;

public class GetMerchantByIdHandler(IMerchantRepository repository, ILogger<GetMerchantByIdHandler> logger)
    : IGetMerchantByIdHandler
{
    public async Task<Result<GetMerchantByIdResult>> HandleAsync(GetMerchantByIdQuery query)
    {
        try
        {
            MerchantId merchantId = MerchantId.Of(query.Id);

            MerchantEntity? merchant = await repository.GetByIdAsync(merchantId, isTrackingEnabled: false);

            if (merchant is null)
                return Result<GetMerchantByIdResult>.Failure($"Catalog with ID {query.Id} not found.");

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
            logger.LogError(ex, "An error occurred while getting merchant by ID.");
            return Result<GetMerchantByIdResult>.Failure("An error occurred while processing your request.");
        }
    }
}