using BuildingBlocks;
using WebApp.Dtos;

namespace WebApp.Services;

public class MarketSpaceService(ILogger<MarketSpaceService> logger, HttpClient httpClient)
    : IMarketSpaceService
{
    public async Task<GetCatalogResponse> GetProductsAsync()
    {
        try
        {
            HttpRequestMessage request = new(HttpMethod.Get, "/api/catalog?pageIndex=1&pageSize=10");

            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            Result<GetCatalogResponse>? result = await response.Content.ReadFromJsonAsync<Result<GetCatalogResponse>>();

            return result?.Data ?? new GetCatalogResponse
            {
                Products = []
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching products from MarketSpaceService");
            return new GetCatalogResponse
            {
                Products = []
            };
        }
    }
}