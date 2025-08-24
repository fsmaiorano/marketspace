using BackendForFrontend.Api.Merchant.Contracts;
using BackendForFrontend.Api.Merchant.Dtos;
using BuildingBlocks;
using Merchant.Api.Application.Merchant.CreateMerchant;
using Merchant.Api.Application.Merchant.DeleteMerchant;
using Merchant.Api.Application.Merchant.GetMerchantById;
using Merchant.Api.Application.Merchant.UpdateMerchant;
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

    public Task<UpdateMerchantResponse> UpdateMerchantAsync(UpdateMerchantRequest request)
    {
        try
        {
            HttpResponseMessage response = httpClient.PutAsJsonAsync($"/merchant/{request.MerchantId}", request).Result;

            if (response.IsSuccessStatusCode)
            {
                Result<UpdateMerchantResult>? resultWrapper =
                    response.Content.ReadFromJsonAsync<Result<UpdateMerchantResult>>().Result;

                if (resultWrapper is { IsSuccess: true, Data: not null })
                {
                    UpdateMerchantResponse merchantResponse = new UpdateMerchantResponse
                    {
                        IsSuccess = resultWrapper.IsSuccess
                    };

                    logger.LogInformation("Merchant updated successfully: {@Merchant}", merchantResponse);
                    return Task.FromResult(merchantResponse);
                }
            }

            logger.LogError("Failed to update merchant. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = response.Content.ReadAsStringAsync().Result;
            throw new HttpRequestException($"Error updating merchant: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating merchant");
            throw;
        }
    }

    public Task<DeleteMerchantResponse> DeleteMerchantAsync(Guid merchantId)
    {
        try
        {
            HttpResponseMessage response = httpClient.DeleteAsync($"/merchant/{merchantId}").Result;

            if (response.IsSuccessStatusCode)
            {
                Result<DeleteMerchantResult>? resultWrapper =
                    response.Content.ReadFromJsonAsync<Result<DeleteMerchantResult>>().Result;

                if (resultWrapper is { IsSuccess: true, Data: not null })
                {
                    DeleteMerchantResponse merchantResponse = new DeleteMerchantResponse
                    {
                        IsSuccess = resultWrapper.IsSuccess
                    };

                    logger.LogInformation("Merchant deleted successfully: {@Merchant}", merchantResponse);
                    return Task.FromResult(merchantResponse);
                }
            }

            logger.LogError("Failed to delete merchant. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = response.Content.ReadAsStringAsync().Result;
            throw new HttpRequestException($"Error deleting merchant: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting merchant");
            throw;
        }
    }

    public Task<GetMerchantByIdResponse> GetMerchantByIdAsync(Guid merchantId)
    {
        try
        {
            HttpResponseMessage response = httpClient.GetAsync($"/merchant/{merchantId}").Result;

            if (response.IsSuccessStatusCode)
            {
                Result<GetMerchantByIdResult>? resultWrapper =
                    response.Content.ReadFromJsonAsync<Result<GetMerchantByIdResult>>().Result;

                if (resultWrapper is { IsSuccess: true, Data: not null })
                {
                    GetMerchantByIdResponse merchantResponse = new GetMerchantByIdResponse
                    {
                        MerchantId = resultWrapper.Data.Id,
                        Name = resultWrapper.Data.Name,
                        Address = resultWrapper.Data.Address
                    };

                    logger.LogInformation("Merchant retrieved successfully: {@Merchant}", merchantResponse);
                    return Task.FromResult(merchantResponse);
                }
            }

            logger.LogError("Failed to retrieve merchant. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = response.Content.ReadAsStringAsync().Result;
            throw new HttpRequestException($"Error retrieving merchant: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while retrieving merchant");
            throw;
        }
    }
}