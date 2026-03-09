using BackendForFrontend.Api.Base;
using BackendForFrontend.Api.Merchant.Dtos;
using BuildingBlocks;
using BuildingBlocks.Loggers;

namespace BackendForFrontend.Api.Merchant.Services;

public class MerchantService(
    IAppLogger<MerchantService> logger,
    HttpClient httpClient,
    IConfiguration configuration)
    : BaseService(httpClient)
{
    private string BaseUrl => configuration["Services:MerchantService:BaseUrl"] ??
                              throw new ArgumentNullException($"MerchantService BaseUrl is not configured");

    public async Task<Result<CreateMerchantResponse>> CreateMerchantAsync(CreateMerchantRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Creating merchant with request: {@Request}", request);

        HttpResponseMessage response = await DoPost($"{BaseUrl}/merchant", request);

        if (response.StatusCode == System.Net.HttpStatusCode.Created)
        {
            logger.LogInformation(LogTypeEnum.Business, "Merchant created successfully");
            return Result<CreateMerchantResponse>.Success(new CreateMerchantResponse());
        }

        logger.LogError(LogTypeEnum.Application, null, "Failed to create merchant. Status code: {StatusCode}", response.StatusCode);
        string errorMessage = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Error creating merchant: {errorMessage}");
    }

    public async Task<Result<UpdateMerchantResponse>> UpdateMerchantAsync(UpdateMerchantRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Updating merchant with request: {@Request}", request);

        HttpResponseMessage response = await DoPut($"{BaseUrl}/merchant", request);

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            logger.LogInformation(LogTypeEnum.Business, "Merchant updated successfully");
            return Result<UpdateMerchantResponse>.Success(new UpdateMerchantResponse { IsSuccess = true });
        }

        logger.LogError(LogTypeEnum.Application, null, "Failed to update merchant. Status code: {StatusCode}", response.StatusCode);
        string errorMessage = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Error updating merchant: {errorMessage}");
    }

    public async Task<Result<DeleteMerchantResponse>> DeleteMerchantAsync(Guid merchantId)
    {
        logger.LogInformation(LogTypeEnum.Application, "Deleting merchant with ID: {MerchantId}", merchantId);

        HttpResponseMessage response = await DoDelete($"{BaseUrl}/merchant/{merchantId}");

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            logger.LogInformation(LogTypeEnum.Business, "Merchant deleted successfully");
            return Result<DeleteMerchantResponse>.Success(new DeleteMerchantResponse { IsSuccess = true });
        }

        logger.LogError(LogTypeEnum.Application, null, "Failed to delete merchant. Status code: {StatusCode}", response.StatusCode);
        string errorMessage = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Error deleting merchant: {errorMessage}");
    }

    public async Task<Result<GetMerchantByIdResponse>> GetMerchantByIdAsync(Guid merchantId)
    {
        logger.LogInformation(LogTypeEnum.Application, "Retrieving merchant with ID: {MerchantId}", merchantId);

        HttpResponseMessage response = await DoGet($"{BaseUrl}/merchant/{merchantId}");

        if (response.IsSuccessStatusCode)
        {
            GetMerchantByIdResponse? merchant = await response.Content.ReadFromJsonAsync<GetMerchantByIdResponse>();
            if (merchant is not null)
            {
                logger.LogInformation(LogTypeEnum.Application, "Merchant retrieved successfully: {@Merchant}", merchant);
                return Result<GetMerchantByIdResponse>.Success(merchant);
            }
            return Result<GetMerchantByIdResponse>.Failure("Merchant not found");
        }

        logger.LogError(LogTypeEnum.Application, null, "Failed to retrieve merchant. Status code: {StatusCode}", response.StatusCode);
        string errorMessage = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Error retrieving merchant: {errorMessage}");
    }
}