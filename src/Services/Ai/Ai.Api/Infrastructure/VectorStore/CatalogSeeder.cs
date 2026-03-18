using System.Text.Json.Serialization;
using Ai.Api.Domain.Embeddings;
using Ai.Api.Domain.Vectors;

namespace Ai.Api.Infrastructure.VectorStore;

public class CatalogSeeder(
    IVectorStore vectorStore,
    IEmbeddingGenerator embeddingGenerator,
    IConfiguration configuration,
    ILogger<CatalogSeeder> logger) : IHostedService
{
    private const string ContextId = "catalog";

    private string CatalogBaseUrl => configuration["Services:CatalogService:BaseUrl"]
                                     ?? throw new ArgumentNullException(
                                         "Services:CatalogService:BaseUrl is not configured");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await SeedCatalogAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "CatalogSeeder failed during startup. Catalog RAG may not work until the catalog service is available.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedCatalogAsync(CancellationToken cancellationToken)
    {
        using HttpClient httpClient = new();
        HttpResponseMessage response =
            await httpClient.GetAsync($"{CatalogBaseUrl}/catalog?PageIndex=1&PageSize=50", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("CatalogSeeder: could not reach catalog API ({Status}). Skipping.",
                response.StatusCode);
            return;
        }

        CatalogPageDto? page = await response.Content.ReadFromJsonAsync<CatalogPageDto>(
            cancellationToken: cancellationToken);

        if (page is null || page.Data.Count == 0)
        {
            logger.LogInformation("CatalogSeeder: no products found. Skipping.");
            return;
        }

        logger.LogInformation("CatalogSeeder: indexing {Count} products into vector store.", page.Data.Count);

        foreach (ProductDto product in page.Data)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string document =
                $"{product.Name}: {product.Description}. " +
                $"Price: {product.Price:C}. " +
                $"Stock: {product.Stock} available. " +
                $"Categories: {string.Join(", ", product.Categories)}.";

            float[] embedding = await embeddingGenerator.Generate(document);
            await vectorStore.Upsert(document, embedding, contextId: ContextId,
                metadata: product.Id.ToString());
        }

        logger.LogInformation("CatalogSeeder: finished indexing {Count} products.", page.Data.Count);
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
