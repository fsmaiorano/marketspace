using BackendForFrontend.Api.Base;
using BackendForFrontend.Api.Merchant.Contracts;
using BackendForFrontend.Api.Merchant.Dtos;
using BuildingBlocks;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace BackendForFrontend.Api.Merchant.Services;

public class MerchantService(ILogger<MerchantService> logger, HttpClient httpClient, IConfiguration configuration)
    : BaseService(httpClient), IMerchantService
{
    private string BaseUrl => configuration["Services:MerchantService:BaseUrl"] ??
                              throw new ArgumentNullException($"MerchantService BaseUrl is not configured");

    public async Task<Result<CreateMerchantResponse>> CreateMerchantAsync(CreateMerchantRequest request)
    {
        logger.LogInformation("Creating merchant with request: {@Request}", request);

        HttpResponseMessage response = await DoPost($"{BaseUrl}/merchant", request);
        Result<CreateMerchantResponse>? content =
            await response.Content.ReadFromJsonAsync<Result<CreateMerchantResponse>>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation("Merchant created successfully: {@Merchant}", content);
            return content;
        }
        else
        {
            logger.LogError("Failed to create merchant. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error updating merchant: {errorMessage}");
        }
    }

    public async Task<Result<UpdateMerchantResponse>> UpdateMerchantAsync(UpdateMerchantRequest request)
    {
        logger.LogInformation("Updating merchant with request: {@Request}", request);

        HttpResponseMessage response = await DoPut($"{BaseUrl}/merchant/{request.Id}", request);
        Result<UpdateMerchantResponse>? content =
            await response.Content.ReadFromJsonAsync<Result<UpdateMerchantResponse>>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation("Merchant updated successfully: {@Merchant}", content);
            return content;
        }
        else
        {
            logger.LogError("Failed to update merchant. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error updating merchant: {errorMessage}");
        }
    }

    public async Task<Result<DeleteMerchantResponse>> DeleteMerchantAsync(Guid merchantId)
    {
        logger.LogInformation("Deleting merchant with ID: {MerchantId}", merchantId);

        HttpResponseMessage response = await DoDelete($"{BaseUrl}/merchant/{merchantId}");
        Result<DeleteMerchantResponse>? content =
            await response.Content.ReadFromJsonAsync<Result<DeleteMerchantResponse>>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation("Merchant deleted successfully: {@Merchant}", content);
            return content;
        }
        else
        {
            logger.LogError("Failed to delete merchant. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error deleting merchant: {errorMessage}");
        }
    }

    public async Task<Result<GetMerchantByIdResponse>> GetMerchantByIdAsync(Guid merchantId)
    {
        logger.LogInformation("Retrieving merchant with ID: {MerchantId}", merchantId);

        HttpResponseMessage response = await DoGet($"{BaseUrl}/merchant/{merchantId}");
        Result<GetMerchantByIdResponse>? content =
            await response.Content.ReadFromJsonAsync<Result<GetMerchantByIdResponse>>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation("Merchant retrieved successfully: {@Merchant}", content);
            return content;
        }
        else
        {
            logger.LogError("Failed to retrieve merchant. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error retrieving merchant: {errorMessage}");
        }
    }
}