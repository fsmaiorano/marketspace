using BackendForFrontend.Api.Base;
using BackendForFrontend.Api.Catalog.Contracts;
using BackendForFrontend.Api.Catalog.Dtos;
using BuildingBlocks;

namespace BackendForFrontend.Api.Catalog.Services;

public class CatalogService(ILogger<CatalogService> logger, HttpClient httpClient, IConfiguration configuration)
    : BaseService(httpClient), ICatalogService
{
    private string BaseUrl => configuration["Services:CatalogService:BaseUrl"] ??
                              throw new ArgumentNullException($"CatalogService BaseUrl is not configured");

    public async Task<Result<CreateCatalogResponse>> CreateCatalogAsync(CreateCatalogRequest request)
    {
        logger.LogInformation("Creating catalog with name: {Name}", request.Name);

        HttpResponseMessage response = await DoPost($"{BaseUrl}/catalog", request);
        Result<CreateCatalogResponse>? content =
            await response.Content.ReadFromJsonAsync<Result<CreateCatalogResponse>>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation("Catalog created successfully: {@Catalog}", content);
            return content;
        }
        else
        {
            logger.LogError("Failed to create catalog. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error creating catalog: {errorMessage}");
        }
    }

    public async Task<Result<GetCatalogResponse>> GetCatalogByIdAsync(Guid catalogId)
    {
        logger.LogInformation("Retrieving catalog with ID: {CatalogId}", catalogId);

        HttpResponseMessage response = await DoGet($"{BaseUrl}/catalog/{catalogId}");
        Result<GetCatalogResponse>? content = await response.Content.ReadFromJsonAsync<Result<GetCatalogResponse>>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation("Catalog retrieved successfully: {@Catalog}", content);
            return content;
        }
        else
        {
            logger.LogError("Failed to retrieve catalog. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error retrieving catalog: {errorMessage}");
        }
    }

    public async Task<Result<GetCatalogListResponse>> GetCatalogListAsync(int pageIndex, int pageSize)
    {
        logger.LogInformation("Retrieving catalog list with pageIndex: {PageIndex}, pageSize: {PageSize}", pageIndex,
            pageSize);

        HttpResponseMessage response = await DoGet($"{BaseUrl}/catalog?pageIndex={pageIndex}&pageSize={pageSize}");
        Result<GetCatalogListResponse>? content =
            await response.Content.ReadFromJsonAsync<Result<GetCatalogListResponse>>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation("Catalog list retrieved successfully with {Count} items",
                content.Data?.Products.Count);
            return content;
        }
        else
        {
            logger.LogError("Failed to retrieve catalog list. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error retrieving catalog list: {errorMessage}");
        }
    }

    public async Task<Result<UpdateCatalogResponse>> UpdateCatalogAsync(UpdateCatalogRequest request)
    {
        logger.LogInformation("Updating catalog with ID: {CatalogId}", request.Id);

        HttpResponseMessage response = await DoPut($"{BaseUrl}/catalog", request);
        Result<UpdateCatalogResponse>? content =
            await response.Content.ReadFromJsonAsync<Result<UpdateCatalogResponse>>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation("Catalog updated successfully: {@Catalog}", content);
            return content;
        }
        else
        {
            logger.LogError("Failed to update catalog. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error updating catalog: {errorMessage}");
        }
    }

    public async Task<Result<DeleteCatalogResponse>> DeleteCatalogAsync(Guid catalogId)
    {
        logger.LogInformation("Deleting catalog with ID: {CatalogId}", catalogId);

        HttpResponseMessage response = await DoDelete($"{BaseUrl}/catalog/{catalogId}");
        Result<DeleteCatalogResponse>? content =
            await response.Content.ReadFromJsonAsync<Result<DeleteCatalogResponse>>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation("Catalog deleted successfully: {@Catalog}", content);
            return content;
        }
        else
        {
            logger.LogError("Failed to delete catalog. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error deleting catalog: {errorMessage}");
        }
    }

    public async Task<Result<GetCatalogListResponse>> GetCatalogListAsync()
    {
        logger.LogInformation("Retrieving catalog list");

        HttpResponseMessage response = await DoGet($"{BaseUrl}/catalog");
        Result<GetCatalogListResponse>? content = await response.Content.ReadFromJsonAsync<Result<GetCatalogListResponse>>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation("Catalog list retrieved successfully with {Count} items", content.Data?.Products.Count);
            return content;
        }
        else
        {
            logger.LogError("Failed to retrieve catalog list. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error retrieving catalog list: {errorMessage}");
        }
    }
}