using BackendForFrontend.Api.Merchant.Contracts;
using BackendForFrontend.Api.Merchant.Dtos;
using BackendForFrontend.Api.Merchant.Services;

namespace BackendForFrontend.Api.Merchant.UseCases;

public class MerchantUseCase(ILogger<MerchantUseCase> logger, IMerchantService service) : IMerchantUseCase
{
    public async Task<CreateMerchantResponse> CreateMerchantAsync(CreateMerchantRequest request)
    {
        logger.LogInformation("Creating merchant with name: {MerchantName}", request.Name);
        return await service.CreateMerchantAsync(request);
    }

    public async Task<UpdateMerchantResponse> UpdateMerchantAsync(UpdateMerchantRequest request)
    {
        logger.LogInformation("Updating merchant with ID: {MerchantId}", request.MerchantId);
        return await service.UpdateMerchantAsync(request);
    }

    public async Task<DeleteMerchantResponse> DeleteMerchantAsync(Guid merchantId)
    {
        logger.LogInformation("Deleting merchant with ID: {MerchantId}", merchantId);
        return await service.DeleteMerchantAsync(merchantId);
    }

    public async Task<GetMerchantByIdResponse> GetMerchantByIdAsync(Guid merchantId)
    {
        logger.LogInformation("Retrieving merchant with ID: {MerchantId}", merchantId);
        return await service.GetMerchantByIdAsync(merchantId);
    }
}