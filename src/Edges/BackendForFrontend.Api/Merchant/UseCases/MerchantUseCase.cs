using BackendForFrontend.Api.Merchant.Dtos;
using BackendForFrontend.Api.Merchant.Services;

namespace BackendForFrontend.Api.Merchant.UseCases;

public interface IMerchantUseCase
{
    Task<CreateMerchantResponse> CreateMerchantAsync(CreateMerchantRequest request); 
}

public class MerchantUseCase(ILogger<MerchantUseCase> logger, IMerchantService service) : IMerchantUseCase
{
    public async Task<CreateMerchantResponse> CreateMerchantAsync(CreateMerchantRequest request)
    {
        return await service.CreateMerchantAsync(request);
    }
}