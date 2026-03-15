using System.Text.Json;
using System.Text.Json.Serialization;
using Ai.Api.Domain.Interfaces;

namespace Ai.Api.Infrastructure.LLM;

public class OllamaClient : ILLMClient
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public OllamaClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(
            configuration["Ollama:BaseUrl"] ?? throw new ArgumentNullException("Ollama:BaseUrl is not configured"));
        _model = configuration["Ollama:GenerationModel"] ?? "mistral";
    }

    public async Task<string> Generate(string prompt)
    {
        var payload = new { model = _model, prompt, stream = false };

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/generate", payload);
        response.EnsureSuccessStatusCode();

        OllamaGenerateResponse? result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>();
        return result?.Response ?? string.Empty;
    }

    private record OllamaGenerateResponse(
        [property: JsonPropertyName("response")] string Response);
}