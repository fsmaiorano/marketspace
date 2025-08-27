namespace WebApp.Services;

public class MarketSpaceService(ILogger<MarketSpaceService> logger, HttpClient httpClient)
    : IMarketSpaceService
{
    public async Task<List<string>> GetProductsAsync()
    {
        try
        {
            HttpRequestMessage request = new(HttpMethod.Get, "/api/catalog?pageIndex=1&pageSize=10");

            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            List<string> products = System.Text.Json.JsonSerializer.Deserialize<List<string>>(content) ?? new List<string>();

            return products;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching products from MarketSpaceService");
            return new List<string>();
        }
    }
}