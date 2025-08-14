using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Application.Merchant.GetMerchantById;

public class GetMerchantByIdHandler(IMerchantRepository merchantRepository, ILogger<GetMerchantByIdHandler> logger)
    : IGetMerchantByIdHandler
{
    public async Task<Result<GetMerchantByIdResult>> HandleAsync(GetMerchantByIdQuery query)
    {
        try
        {
            MerchantId merchantId = MerchantId.Of(query.Id);

            MerchantEntity? merchant = await merchantRepository.GetByIdAsync(merchantId, isTrackingEnabled: false);

            if (merchant is null)
                return Result<GetMerchantByIdResult>.Failure($"Merchant with ID {query.Id} not found.");

            GetMerchantByIdResult result = new
            (
                merchant.Id.Value,
                merchant.Name,
                merchant.Email.Value,
                merchant.PhoneNumber,
                merchant.Address
            );

            return Result<GetMerchantByIdResult>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while getting merchant by ID.");
            return Result<GetMerchantByIdResult>.Failure("An error occurred while processing your request.");
        }
    }
}