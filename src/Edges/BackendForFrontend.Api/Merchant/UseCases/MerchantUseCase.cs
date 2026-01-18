using BackendForFrontend.Api.Merchant.Contracts;
using BackendForFrontend.Api.Merchant.Dtos;
using BuildingBlocks;
using BuildingBlocks.Loggers;

namespace BackendForFrontend.Api.Merchant.UseCases;

public class MerchantUseCase(
    IAppLogger<MerchantUseCase> logger,
    IMerchantService service) : IMerchantUseCase
{
    public async Task<Result<CreateMerchantResponse>> CreateMerchantAsync(CreateMerchantRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Creating merchant with name: {MerchantName}", request.Name);
        return await service.CreateMerchantAsync(request);
    }

    public async Task<Result<UpdateMerchantResponse>> UpdateMerchantAsync(UpdateMerchantRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Updating merchant with ID: {MerchantId}", request.Id);
        return await service.UpdateMerchantAsync(request);
    }

    public async Task<Result<DeleteMerchantResponse>> DeleteMerchantAsync(Guid merchantId)
    {
        logger.LogInformation(LogTypeEnum.Application, "Deleting merchant with ID: {MerchantId}", merchantId);
        return await service.DeleteMerchantAsync(merchantId);
    }

    public async Task<Result<GetMerchantByIdResponse>> GetMerchantByIdAsync(Guid merchantId)
    {
        logger.LogInformation(LogTypeEnum.Application, "Retrieving merchant with ID: {MerchantId}", merchantId);
        return await service.GetMerchantByIdAsync(merchantId);
    }
}