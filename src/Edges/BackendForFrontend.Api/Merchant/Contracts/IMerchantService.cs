using BackendForFrontend.Api.Merchant.Dtos;

namespace BackendForFrontend.Api.Merchant.Contracts;

public interface IMerchantService
{
    Task<CreateMerchantResponse> CreateMerchantAsync(CreateMerchantRequest request);
    Task<UpdateMerchantResponse> UpdateMerchantAsync(UpdateMerchantRequest request);
    Task<DeleteMerchantResponse> DeleteMerchantAsync(Guid merchantId);
    Task<GetMerchantByIdResponse> GetMerchantByIdAsync(Guid merchantId);
}