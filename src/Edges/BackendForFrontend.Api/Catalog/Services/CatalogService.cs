using BackendForFrontend.Api.Base;
using BackendForFrontend.Api.Catalog.Dtos;
using BuildingBlocks;
using BuildingBlocks.Loggers;
using BuildingBlocks.Pagination;

namespace BackendForFrontend.Api.Catalog.Services;

public class CatalogService(
    IAppLogger<CatalogService> logger,
    HttpClient httpClient,
    IConfiguration configuration)
    : BaseService(httpClient)
{
    private string BaseUrl => configuration["Services:CatalogService:BaseUrl"] ??
                              throw new ArgumentNullException($"CatalogService BaseUrl is not configured");

    public async Task<Result<CreateCatalogResponse>> CreateCatalogAsync(CreateCatalogRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Creating catalog with name: {Name}", request.Name);

        HttpResponseMessage response = await DoPost($"{BaseUrl}/catalog", request);

        if (response.StatusCode == System.Net.HttpStatusCode.Created)
        {
            logger.LogInformation(LogTypeEnum.Business, "Catalog created successfully");
            return Result<CreateCatalogResponse>.Success(new CreateCatalogResponse());
        }

        logger.LogError(LogTypeEnum.Application, null, "Failed to create catalog. Status code: {StatusCode}", response.StatusCode);
        string errorMessage = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Error creating catalog: {errorMessage}");
    }

    public async Task<Result<GetCatalogResponse>> GetCatalogByIdAsync(Guid catalogId)
    {
        logger.LogInformation(LogTypeEnum.Application, "Retrieving catalog with ID: {CatalogId}", catalogId);

        HttpResponseMessage response = await DoGet($"{BaseUrl}/catalog/{catalogId}");

        if (response.IsSuccessStatusCode)
        {
            GetCatalogResponse? item = await response.Content.ReadFromJsonAsync<GetCatalogResponse>();
            if (item is not null)
            {
                logger.LogInformation(LogTypeEnum.Application, "Catalog retrieved successfully: {@Catalog}", item);
                return Result<GetCatalogResponse>.Success(item);
            }
            return Result<GetCatalogResponse>.Failure("Catalog not found");
        }

        logger.LogError(LogTypeEnum.Application, null, "Failed to retrieve catalog. Status code: {StatusCode}", response.StatusCode);
        string errorMessage = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Error retrieving catalog: {errorMessage}");
    }

    public async Task<Result<GetCatalogListResponse>> GetCatalogListAsync(int pageIndex, int pageSize)
    {
        logger.LogInformation(LogTypeEnum.Application, "Retrieving catalog list with pageIndex: {PageIndex}, pageSize: {PageSize}", pageIndex,
            pageSize);

        HttpResponseMessage response = await DoGet($"{BaseUrl}/catalog?pageIndex={pageIndex}&pageSize={pageSize}");

        if (response.IsSuccessStatusCode)
        {
            PaginatedResult<CatalogItemDto>? paginated =
                await response.Content.ReadFromJsonAsync<PaginatedResult<CatalogItemDto>>();

            if (paginated is null)
                throw new HttpRequestException("Empty response from catalog service");

            GetCatalogListResponse mapped = new()
            {
                PageIndex = paginated.PageIndex,
                PageSize = paginated.PageSize,
                Count = paginated.Count,
                Products = paginated.Data.Select(p => new CatalogDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    Categories = p.Categories,
                    MerchantId = p.MerchantId,
                    Stock = p.Stock,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList()
            };

            logger.LogInformation(LogTypeEnum.Application, "Catalog list retrieved successfully with {Count} items", mapped.Products.Count);
            return Result<GetCatalogListResponse>.Success(mapped);
        }
        else
        {
            logger.LogError(LogTypeEnum.Application, null, "Failed to retrieve catalog list. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error retrieving catalog list: {errorMessage}");
        }
    }

    public async Task<Result<UpdateCatalogResponse>> UpdateCatalogAsync(UpdateCatalogRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Updating catalog with ID: {CatalogId}", request.Id);

        HttpResponseMessage response = await DoPut($"{BaseUrl}/catalog", request);

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            logger.LogInformation(LogTypeEnum.Business, "Catalog updated successfully");
            return Result<UpdateCatalogResponse>.Success(new UpdateCatalogResponse());
        }

        logger.LogError(LogTypeEnum.Application, null, "Failed to update catalog. Status code: {StatusCode}", response.StatusCode);
        string errorMessage = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Error updating catalog: {errorMessage}");
    }

    public async Task<Result<DeleteCatalogResponse>> DeleteCatalogAsync(Guid catalogId)
    {
        logger.LogInformation(LogTypeEnum.Application, "Deleting catalog with ID: {CatalogId}", catalogId);

        HttpResponseMessage response = await DoDelete($"{BaseUrl}/catalog/{catalogId}");

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            logger.LogInformation(LogTypeEnum.Business, "Catalog deleted successfully");
            return Result<DeleteCatalogResponse>.Success(new DeleteCatalogResponse { IsSuccess = true });
        }

        logger.LogError(LogTypeEnum.Application, null, "Failed to delete catalog. Status code: {StatusCode}", response.StatusCode);
        string errorMessage = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Error deleting catalog: {errorMessage}");
    }

    public async Task<Result<GetCatalogListResponse>> GetCatalogByMerchantIdAsync(Guid merchantId, int pageIndex, int pageSize)
    {
        logger.LogInformation(LogTypeEnum.Application, "Retrieving catalog for merchant {MerchantId}", merchantId);

        HttpResponseMessage response = await DoGet($"{BaseUrl}/catalog/merchant/{merchantId}?pageIndex={pageIndex}&pageSize={pageSize}");

        if (response.IsSuccessStatusCode)
        {
            PaginatedResult<CatalogItemDto>? paginated =
                await response.Content.ReadFromJsonAsync<PaginatedResult<CatalogItemDto>>();

            if (paginated is null)
                throw new HttpRequestException("Empty response from catalog service");

            GetCatalogListResponse mapped = new()
            {
                PageIndex = paginated.PageIndex,
                PageSize = paginated.PageSize,
                Count = paginated.Count,
                Products = paginated.Data.Select(p => new CatalogDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    Categories = p.Categories,
                    MerchantId = p.MerchantId,
                    Stock = p.Stock,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList()
            };

            logger.LogInformation(LogTypeEnum.Application, "Merchant catalog retrieved successfully with {Count} items", mapped.Products.Count);
            return Result<GetCatalogListResponse>.Success(mapped);
        }

        string errorMessage = await response.Content.ReadAsStringAsync();
        return Result<GetCatalogListResponse>.Failure($"Failed to retrieve merchant catalog: {errorMessage}");
    }

    public async Task<Result<UpdateStockResponse>> UpdateStockAsync(Guid catalogId, int delta)
    {
        logger.LogInformation(LogTypeEnum.Application, "Updating stock for catalog {CatalogId}, delta: {Delta}", catalogId, delta);

        HttpResponseMessage response = await DoPatch($"{BaseUrl}/catalog/{catalogId}/stock", new { delta });

        if (response.IsSuccessStatusCode)
        {
            UpdateStockResponse? result = await response.Content.ReadFromJsonAsync<UpdateStockResponse>();
            logger.LogInformation(LogTypeEnum.Business, "Stock updated successfully for catalog {CatalogId}", catalogId);
            return Result<UpdateStockResponse>.Success(result ?? new UpdateStockResponse());
        }

        string errorMessage = await response.Content.ReadAsStringAsync();
        return Result<UpdateStockResponse>.Failure($"Failed to update stock: {errorMessage}");
    }
}