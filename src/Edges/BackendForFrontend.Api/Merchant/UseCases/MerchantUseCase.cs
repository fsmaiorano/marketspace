using BackendForFrontend.Api.Merchant.Contracts;
using BackendForFrontend.Api.Merchant.Dtos;
using BackendForFrontend.Api.Merchant.Services;

namespace BackendForFrontend.Api.Merchant.UseCases;

public class MerchantUseCase(ILogger<MerchantUseCase> logger, IMerchantService service) : IMerchantUseCase
{
    public async Task<CreateMerchantResponse> CreateMerchantAsync(CreateMerchantRequest request)
    {
        return await service.CreateMerchantAsync(request);
    }

    public async Task<UpdateMerchantResponse> UpdateMerchantAsync(UpdateMerchantRequest request)
    {
        return await service.UpdateMerchantAsync(request);
    }

    public async Task<DeleteMerchantResponse> DeleteMerchantAsync(Guid merchantId)
    {
        return await service.DeleteMerchantAsync(merchantId);
    }

    public async Task<GetMerchantByIdResponse> GetMerchantByIdAsync(Guid merchantId)
    {
        return await service.GetMerchantByIdAsync(merchantId);
    }
}