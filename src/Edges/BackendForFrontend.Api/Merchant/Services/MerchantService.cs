using BackendForFrontend.Api.Base;
using BackendForFrontend.Api.Merchant.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace BackendForFrontend.Api.Merchant.Services;

public interface IMerchantService
{
    Task<CreateMerchantResponse> CreateMerchantAsync(CreateMerchantRequest request);
}

public class MerchantService(ILogger<MerchantService> logger, HttpClient httpClient, IConfiguration configuration)
    : BaseService(httpClient), IMerchantService
{
    private string BaseUrl => configuration["Services:MerchantService:BaseUrl"] ?? throw new ArgumentNullException("Services:Merchant:Url");

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
}