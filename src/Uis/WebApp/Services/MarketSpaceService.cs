using BuildingBlocks;
using WebApp.Dtos;

namespace WebApp.Services;

public class MarketSpaceService(ILogger<MarketSpaceService> logger, HttpClient httpClient)
    : IMarketSpaceService
{
    public async Task<GetCatalogResponse> GetProductsAsync()
    {
        return await GetProductsAsync(1, 10);
    }

    public async Task<GetCatalogResponse> GetProductsAsync(int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            HttpRequestMessage request = new(HttpMethod.Get, $"/api/catalog?pageIndex={page}&pageSize={pageSize}");

            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            Result<GetCatalogResponse>? result =
                await response.Content.ReadFromJsonAsync<Result<GetCatalogResponse>>(
                    cancellationToken: cancellationToken);

            return result?.Data ?? new GetCatalogResponse
            {
                Products = [], PageIndex = page, PageSize = pageSize, Count = 0
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching products from MarketSpaceService");
            return new GetCatalogResponse { Products = [], PageIndex = page, PageSize = pageSize, Count = 0 };
        }
    }

    public async Task<CatalogDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpRequestMessage request = new(HttpMethod.Get, $"/api/catalog/{id}");

            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            Result<CatalogDto>? result =
                await response.Content.ReadFromJsonAsync<Result<CatalogDto>>(cancellationToken: cancellationToken);

            return result?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching product by ID from MarketSpaceService");
            return null;
        }
    }

    public async Task CreateOrUpdateBasketAsync(CreateOrUpdateBasketRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            HttpRequestMessage requestMessage = new(HttpMethod.Post, "/api/basket")
            {
                Content = JsonContent.Create(request)
            };

            using HttpResponseMessage response = await httpClient.SendAsync(requestMessage, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating or updating basket in MarketSpaceService");
        }
    }

    public async Task<GetBasketResponse?>? GetBasketByUsernameAsync(string username,
        CancellationToken cancellationToken = default)
    {
        try
        {
            HttpRequestMessage request = new(HttpMethod.Get, $"/api/basket/{username}");

            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            Result<GetBasketResponse>? result =
                await response.Content.ReadFromJsonAsync<Result<GetBasketResponse>>(
                    cancellationToken: cancellationToken);

            return result?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching basket by username from MarketSpaceService");
            return null;
        }
    }
}