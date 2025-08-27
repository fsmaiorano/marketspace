using BackendForFrontend.Api.Merchant.Dtos;
using BuildingBlocks;

namespace BackendForFrontend.Api.Merchant.Contracts;

public interface IMerchantService
{
    Task<Result<CreateMerchantResponse>> CreateMerchantAsync(CreateMerchantRequest request);
    Task<Result<UpdateMerchantResponse>> UpdateMerchantAsync(UpdateMerchantRequest request);
    Task<Result<DeleteMerchantResponse>> DeleteMerchantAsync(Guid merchantId);
    Task<Result<GetMerchantByIdResponse>> GetMerchantByIdAsync(Guid merchantId);
}