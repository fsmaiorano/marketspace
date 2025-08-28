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

    public async Task<GetCatalogResponse> GetProductsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpRequestMessage request = new(HttpMethod.Get, $"/api/catalog?pageIndex={page}&pageSize={pageSize}");

            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            Result<GetCatalogResponse>? result = await response.Content.ReadFromJsonAsync<Result<GetCatalogResponse>>(cancellationToken: cancellationToken);

            return result?.Data ?? new GetCatalogResponse
            {
                Products = [],
                PageIndex = page,
                PageSize = pageSize,
                Count = 0
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching products from MarketSpaceService");
            return new GetCatalogResponse
            {
                Products = [],
                PageIndex = page,
                PageSize = pageSize,
                Count = 0
            };
        }
    }
}