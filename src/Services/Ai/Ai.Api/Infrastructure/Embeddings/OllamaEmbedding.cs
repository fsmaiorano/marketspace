using System.Text.Json.Serialization;
using Ai.Api.Domain.Embeddings;

namespace Ai.Api.Infrastructure.Embeddings;

public class OllamaEmbedding : IEmbeddingGenerator
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public OllamaEmbedding(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(
            configuration["Ollama:BaseUrl"] ?? throw new ArgumentNullException("Ollama:BaseUrl is not configured"));
        _model = configuration["Ollama:EmbeddingModel"] ?? "nomic-embed-text";
    }

    public async Task<float[]> Generate(string text)
    {
        var payload = new { model = _model, prompt = text };

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/embeddings", payload);
        response.EnsureSuccessStatusCode();

        OllamaEmbeddingResponse? result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>();
        return result?.Embedding ?? [];
    }

    private record OllamaEmbeddingResponse(
        [property: JsonPropertyName("embedding")] float[] Embedding);
}