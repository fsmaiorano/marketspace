using BackendForFrontend.Api.Catalog.Contracts;
using BackendForFrontend.Api.Catalog.Dtos;
using Builder;
using BuildingBlocks;
using Catalog.Api.Application.Catalog.CreateCatalog;
using Catalog.Api.Application.Catalog.DeleteCatalog;
using Catalog.Api.Application.Catalog.GetCatalog;
using Catalog.Api.Application.Catalog.GetCatalogById;
using Catalog.Api.Application.Catalog.UpdateCatalog;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace BackendForFrontend.Test.Mocks;

public class TestCatalogService(HttpClient httpClient, ILogger<TestCatalogService> logger) : ICatalogService
{
    public async Task<Result<CreateCatalogResponse>> CreateCatalogAsync(CreateCatalogRequest request)
    {
        try
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/catalog", request);

            if (response.IsSuccessStatusCode)
            {
                Result<CreateCatalogResult>? resultWrapper =
                    await response.Content.ReadFromJsonAsync<Result<CreateCatalogResult>>();

                if (resultWrapper is { IsSuccess: true, Data: not null })
                {
                    Result<CreateCatalogResponse> catalogResponse = Result<CreateCatalogResponse>.Success(
                        new CreateCatalogResponse
                        {
                            Id = resultWrapper.Data.CatalogId,
                            Name = request.Name,
                            Description = request.Description,
                            Price = request.Price,
                            Categories = request.Categories,
                            ImageUrl = request.ImageUrl,
                            MerchantId = request.MerchantId
                        });
                    logger.LogInformation("Catalog created successfully: {@Catalog}", catalogResponse);

                    return catalogResponse;
                }
            }

            logger.LogError("Failed to create catalog. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error creating catalog: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating catalog");
            throw;
        }
    }

    public async Task<Result<GetCatalogResponse>> GetCatalogByIdAsync(Guid catalogId)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync($"/catalog/{catalogId}");

            if (response.IsSuccessStatusCode)
            {
                Result<GetCatalogByIdResult>? resultWrapper =
                    await response.Content.ReadFromJsonAsync<Result<GetCatalogByIdResult>>();

                if (resultWrapper is { IsSuccess: true, Data: not null })
                {
                    GetCatalogByIdResult catalogData = resultWrapper.Data;
                    Result<GetCatalogResponse> catalogResponse = Result<GetCatalogResponse>.Success(
                        new GetCatalogResponse
                        {
                            Id = catalogData.Id,
                            Name = catalogData.Name,
                            Description = catalogData.Description,
                            Price = catalogData.Price,
                            Categories = catalogData.Categories.ToList(),
                            ImageUrl = catalogData.ImageUrl,
                            MerchantId = catalogData.MerchantId,
                            CreatedAt = catalogData.CreatedAt,
                            UpdatedAt = catalogData.UpdatedAt
                        });

                    logger.LogInformation("Catalog retrieved successfully: {@Catalog}", catalogResponse);
                    return catalogResponse;
                }
            }

            logger.LogError("Failed to retrieve catalog. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error retrieving catalog: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while retrieving catalog with ID: {CatalogId}", catalogId);
            throw;
        }
    }

    public async Task<Result<GetCatalogListResponse>> GetCatalogListAsync(int pageIndex, int pageSize)
    {
        try
        {
            HttpResponseMessage response =
                await httpClient.GetAsync($"/catalog?pageIndex={pageIndex}&pageSize={pageSize}");

            if (response.IsSuccessStatusCode)
            {
                Result<GetCatalogResult>? resultWrapper =
                    response.Content.ReadFromJsonAsync<Result<GetCatalogResult>>().Result;

                if (resultWrapper is { IsSuccess: true, Data: not null })
                {
                    Result<GetCatalogListResponse> catalogListResponse = Result<GetCatalogListResponse>.Success(
                        new GetCatalogListResponse
                        {
                            PageIndex = pageIndex,
                            PageSize = pageSize,
                            Count = resultWrapper.Data.Count,
                            Products = resultWrapper.Data.Products.Select(item => new CatalogDto
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Description = item.Description,
                                Price = item.Price,
                                Categories = item.Categories.ToList(),
                                ImageUrl = item.ImageUrl,
                                MerchantId = item.MerchantId,
                                CreatedAt = item.CreatedAt,
                                UpdatedAt = item.UpdatedAt
                            }).ToList()
                        });

                    logger.LogInformation("Catalog list retrieved successfully: {@CatalogList}", catalogListResponse);
                    return catalogListResponse;
                }
            }

            logger.LogError("Failed to retrieve catalog list. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = response.Content.ReadAsStringAsync().Result;
            throw new HttpRequestException($"Error retrieving catalog list: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while retrieving catalog list");
            throw;
        }
    }

    public async Task<Result<UpdateCatalogResponse>> UpdateCatalogAsync(UpdateCatalogRequest request)
    {
        try
        {
            HttpResponseMessage response = await httpClient.PutAsJsonAsync($"/catalog", request);

            if (response.IsSuccessStatusCode)
            {
                Result<UpdateCatalogResult>? resultWrapper =
                    response.Content.ReadFromJsonAsync<Result<UpdateCatalogResult>>().Result;

                if (resultWrapper is { IsSuccess: true, Data: not null })
                {
                    Result<UpdateCatalogResponse> catalogResponse = Result<UpdateCatalogResponse>.Success(
                        new UpdateCatalogResponse
                        {
                            Id = request.Id,
                            Name = request.Name,
                            Description = request.Description,
                            Price = request.Price,
                            Categories = request.Categories,
                            ImageUrl = request.ImageUrl,
                            MerchantId = request.MerchantId
                        });

                    logger.LogInformation("Catalog updated successfully: {@Catalog}", catalogResponse);
                    return catalogResponse;
                }
            }

            logger.LogError("Failed to update catalog. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = response.Content.ReadAsStringAsync().Result;
            throw new HttpRequestException($"Error updating catalog: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating catalog with ID: {CatalogId}", request.Id);
            throw;
        }
    }


    public async Task<Result<DeleteCatalogResponse>> DeleteCatalogAsync(Guid catalogId)
    {
        try
        {
            DeleteCatalogCommand command = CatalogBuilder.CreateDeleteCatalogCommandFaker(catalogId).Generate();
            HttpRequestMessage request = new(HttpMethod.Delete, "/catalog") { Content = JsonContent.Create(command) };

            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                Result<DeleteCatalogResult>? resultWrapper =
                    response.Content.ReadFromJsonAsync<Result<DeleteCatalogResult>>().Result;

                if (resultWrapper is { IsSuccess: true, Data: not null })
                {
                    Result<DeleteCatalogResponse> catalogResponse = Result<DeleteCatalogResponse>.Success(
                        new DeleteCatalogResponse { IsSuccess = resultWrapper.IsSuccess });

                    logger.LogInformation("Catalog deleted successfully: {@Catalog}", catalogResponse);
                    return catalogResponse;
                }
            }

            logger.LogError("Failed to delete catalog. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = response.Content.ReadAsStringAsync().Result;
            throw new HttpRequestException($"Error deleting catalog: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting catalog with ID: {CatalogId}", catalogId);
            throw;
        }
    }
}