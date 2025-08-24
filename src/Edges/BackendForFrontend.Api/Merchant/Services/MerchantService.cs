using BackendForFrontend.Api.Base;
using BackendForFrontend.Api.Merchant.Contracts;
using BackendForFrontend.Api.Merchant.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace BackendForFrontend.Api.Merchant.Services;

public class MerchantService(ILogger<MerchantService> logger, HttpClient httpClient, IConfiguration configuration)
    : BaseService(httpClient), IMerchantService
{
    private string BaseUrl => configuration["Services:MerchantService:BaseUrl"] ??
                              throw new ArgumentNullException($"MerchantService BaseUrl is not configured");

    public async Task<CreateMerchantResponse> CreateMerchantAsync(CreateMerchantRequest request)
    {
        HttpResponseMessage response = await DoPost($"{BaseUrl}/merchant", request);
        CreateMerchantResponse? content = await response.Content.ReadFromJsonAsync<CreateMerchantResponse>();

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
            throw new HttpRequestException($"Error creating merchant: {errorMessage}");
        }
    }

    public async Task<UpdateMerchantResponse> UpdateMerchantAsync(UpdateMerchantRequest request)
    {
        HttpResponseMessage response = await DoPut($"{BaseUrl}/merchant/{request.MerchantId}", request);
        UpdateMerchantResponse? content = await response.Content.ReadFromJsonAsync<UpdateMerchantResponse>();

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

    public async Task<DeleteMerchantResponse> DeleteMerchantAsync(Guid merchantId)
    {
        HttpResponseMessage response = await DoDelete($"{BaseUrl}/merchant/{merchantId}");
        DeleteMerchantResponse? content = await response.Content.ReadFromJsonAsync<DeleteMerchantResponse>();

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

    public async Task<GetMerchantByIdResponse> GetMerchantByIdAsync(Guid merchantId)
    {
        HttpResponseMessage response = await DoGet($"{BaseUrl}/merchant/{merchantId}");
        GetMerchantByIdResponse? content = await response.Content.ReadFromJsonAsync<GetMerchantByIdResponse>();

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