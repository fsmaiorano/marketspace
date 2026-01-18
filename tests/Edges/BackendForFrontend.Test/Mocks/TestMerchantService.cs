using BackendForFrontend.Api.Merchant.Contracts;
using Merchant.Api.Application.Merchant.CreateMerchant;
using Merchant.Api.Application.Merchant.DeleteMerchant;
using Merchant.Api.Application.Merchant.GetMerchantById;
using Merchant.Api.Application.Merchant.UpdateMerchant;

namespace BackendForFrontend.Test.Mocks;

public class TestMerchantService(
    HttpClient httpClient, 
    IAppLogger<TestMerchantService> logger) : IMerchantService
{
    public async Task<Result<CreateMerchantResponse>> CreateMerchantAsync(CreateMerchantRequest request)
    {
        try
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/merchant", request);

            if (response.IsSuccessStatusCode)
            {
                Result<CreateMerchantResult>? resultWrapper =
                    await response.Content.ReadFromJsonAsync<Result<CreateMerchantResult>>();
                if (resultWrapper is { IsSuccess: true, Data: not null })
                {
                    Result<CreateMerchantResponse> merchantResponse =
                        Result<CreateMerchantResponse>.Success(new CreateMerchantResponse
                        {
                            MerchantId = resultWrapper.Data.MerchantId
                        });

                    logger.LogInformation(LogTypeEnum.Application, "Merchant created successfully: {@Merchant}", merchantResponse);
                    return merchantResponse;
                }
            }

            logger.LogError(LogTypeEnum.Application, null, "Failed to create merchant. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error creating merchant: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "Error occurred while creating merchant");
            throw;
        }
    }

    public async Task<Result<UpdateMerchantResponse>> UpdateMerchantAsync(UpdateMerchantRequest request)
    {
        try
        {
            HttpResponseMessage response = await httpClient.PutAsJsonAsync($"/merchant", request);

            if (response.IsSuccessStatusCode)
            {
                Result<UpdateMerchantResult>? resultWrapper = await
                    response.Content.ReadFromJsonAsync<Result<UpdateMerchantResult>>();

                if (resultWrapper is { IsSuccess: true, Data: not null })
                {
                    Result<UpdateMerchantResponse> merchantResponse =
                        Result<UpdateMerchantResponse>.Success(new UpdateMerchantResponse
                        {
                            IsSuccess = resultWrapper.IsSuccess
                        });

                    logger.LogInformation(LogTypeEnum.Application, "Merchant updated successfully: {@Merchant}", merchantResponse);
                    return merchantResponse;
                }
            }

            logger.LogError(LogTypeEnum.Application, null, "Failed to update merchant. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = response.Content.ReadAsStringAsync().Result;
            throw new HttpRequestException($"Error updating merchant: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "Error occurred while updating merchant");
            throw;
        }
    }

    public async Task<Result<DeleteMerchantResponse>> DeleteMerchantAsync(Guid merchantId)
    {
        try
        {
            HttpResponseMessage response = await httpClient.DeleteAsync($"/merchant/{merchantId}");

            if (response.IsSuccessStatusCode)
            {
                Result<DeleteMerchantResult>? resultWrapper = await
                    response.Content.ReadFromJsonAsync<Result<DeleteMerchantResult>>();

                if (resultWrapper is { IsSuccess: true, Data: not null })
                {
                    Result<DeleteMerchantResponse> merchantResponse =
                        Result<DeleteMerchantResponse>.Success(new DeleteMerchantResponse
                        {
                            IsSuccess = resultWrapper.IsSuccess
                        });

                    logger.LogInformation(LogTypeEnum.Application, "Merchant deleted successfully: {@Merchant}", merchantResponse);
                    return merchantResponse;
                }
            }

            logger.LogError(LogTypeEnum.Application, null, "Failed to delete merchant. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = response.Content.ReadAsStringAsync().Result;
            throw new HttpRequestException($"Error deleting merchant: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "Error occurred while deleting merchant");
            throw;
        }
    }

    public async Task<Result<GetMerchantByIdResponse>> GetMerchantByIdAsync(Guid merchantId)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync($"/merchant/{merchantId}");

            if (response.IsSuccessStatusCode)
            {
                Result<GetMerchantByIdResult>? resultWrapper = await
                    response.Content.ReadFromJsonAsync<Result<GetMerchantByIdResult>>();

                if (resultWrapper is { IsSuccess: true, Data: not null })
                {
                    Result<GetMerchantByIdResponse> merchantResponse =
                        Result<GetMerchantByIdResponse>.Success(new GetMerchantByIdResponse
                        {
                            MerchantId = resultWrapper.Data.Id,
                            Name = resultWrapper.Data.Name,
                            Address = resultWrapper.Data.Address
                        });

                    logger.LogInformation(LogTypeEnum.Application, "Merchant retrieved successfully: {@Merchant}", merchantResponse);
                    return merchantResponse;
                }
            }

            logger.LogError(LogTypeEnum.Application, null, "Failed to retrieve merchant. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = response.Content.ReadAsStringAsync().Result;
            throw new HttpRequestException($"Error retrieving merchant: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "Error occurred while retrieving merchant");
            throw;
        }
    }
}