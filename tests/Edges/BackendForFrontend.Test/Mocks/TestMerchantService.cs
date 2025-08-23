using BackendForFrontend.Api.Merchant.Dtos;
using BackendForFrontend.Api.Merchant.Services;
using BuildingBlocks;
using Merchant.Api.Application.Merchant.CreateMerchant;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace BackendForFrontend.Test.Mocks;

public class TestMerchantService(HttpClient httpClient, ILogger<TestMerchantService> logger) : IMerchantService
{
    public async Task<CreateMerchantResponse> CreateMerchantAsync(CreateMerchantRequest request)
    {
        try
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/merchant", request);

            if (response.IsSuccessStatusCode)
            {
                Result<CreateMerchantResult>? resultWrapper =
                    await response.Content.ReadFromJsonAsync<Result<CreateMerchantResult>>();
                if (resultWrapper?.IsSuccess == true && resultWrapper.Data != null)
                {
                    CreateMerchantResponse merchantResponse = new CreateMerchantResponse
                    {
                        MerchantId = resultWrapper.Data.MerchantId
                    };

                    logger.LogInformation("Merchant created successfully: {@Merchant}", merchantResponse);
                    return merchantResponse;
                }
            }

            logger.LogError("Failed to create merchant. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error creating merchant: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating merchant");
            throw;
        }
    }
}