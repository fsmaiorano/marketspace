using BackendForFrontend.Api.Merchant.Contracts;
using BackendForFrontend.Api.Merchant.Dtos;
using BuildingBlocks;
using BuildingBlocks.Loggers.Abstractions;

namespace BackendForFrontend.Api.Merchant.UseCases;

public class MerchantUseCase(
    IApplicationLogger<MerchantUseCase> applicationLogger,
    IBusinessLogger<MerchantUseCase> businessLogger,
    IMerchantService service) : IMerchantUseCase
{
    public async Task<Result<CreateMerchantResponse>> CreateMerchantAsync(CreateMerchantRequest request)
    {
        applicationLogger.LogInformation("Creating merchant with name: {MerchantName}", request.Name);
        return await service.CreateMerchantAsync(request);
    }

    public async Task<Result<UpdateMerchantResponse>> UpdateMerchantAsync(UpdateMerchantRequest request)
    {
        applicationLogger.LogInformation("Updating merchant with ID: {MerchantId}", request.Id);
        return await service.UpdateMerchantAsync(request);
    }

    public async Task<Result<DeleteMerchantResponse>> DeleteMerchantAsync(Guid merchantId)
    {
        applicationLogger.LogInformation("Deleting merchant with ID: {MerchantId}", merchantId);
        return await service.DeleteMerchantAsync(merchantId);
    }

    public async Task<Result<GetMerchantByIdResponse>> GetMerchantByIdAsync(Guid merchantId)
    {
        applicationLogger.LogInformation("Retrieving merchant with ID: {MerchantId}", merchantId);
        return await service.GetMerchantByIdAsync(merchantId);
    }
}