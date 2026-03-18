using System.Text.Json.Serialization;

namespace Ai.Api.Application.Tools;

public class SearchProductsTool(HttpClient httpClient, IConfiguration configuration)
{
    private string BaseUrl => configuration["Services:CatalogService:BaseUrl"]
                              ?? throw new ArgumentNullException("Services:CatalogService:BaseUrl is not configured");

    public async Task<string?> SearchAsync(int pageSize = 20)
    {
        try
        {
            HttpResponseMessage response =
                await httpClient.GetAsync($"{BaseUrl}/catalog?PageIndex=1&PageSize={pageSize}");

            if (!response.IsSuccessStatusCode)
                return null;

            CatalogPageDto? page = await response.Content.ReadFromJsonAsync<CatalogPageDto>();

            if (page is null || page.Data.Count == 0)
                return "No products found in the catalog.";

            IEnumerable<string> lines = page.Data.Select(p =>
                $"- [{p.Id}] {p.Name} | Price: {p.Price:C} | Stock: {p.Stock} | Categories: {string.Join(", ", p.Categories)} | {p.Description}");

            return string.Join("\n", lines);
        }
        catch
        {
            return null;
        }
    }

    private record CatalogPageDto(
        [property: JsonPropertyName("data")] List<ProductDto> Data);

    private record ProductDto(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("price")] decimal Price,
        [property: JsonPropertyName("stock")] int Stock,
        [property: JsonPropertyName("categories")] List<string> Categories);
}
