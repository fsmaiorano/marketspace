using System.Text.Json.Serialization;
using Ai.Api.Domain.Interfaces;

namespace Ai.Api.Infrastructure.LLM;

public class OllamaClient : ILLMClient
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly int _numPredict;

    public OllamaClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(
            configuration["Ollama:BaseUrl"] ?? throw new ArgumentNullException("Ollama:BaseUrl is not configured"));
        _httpClient.Timeout = TimeSpan.FromSeconds(300);
        _model = configuration["Ollama:GenerationModel"] ?? "llama3.2:1b";
        _numPredict = configuration.GetValue<int>("Ollama:NumPredict", 128);
    }

    public async Task<string> Generate(string prompt)
    {
        // llama3.2:1b is a chat-tuned model — use /api/chat with messages format
        var payload = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            stream = false,
            options = new
            {
                num_predict = _numPredict,
                temperature = 0.7f,
                top_p = 0.9f,
                top_k = 40
            }
        };

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/chat", payload);
        response.EnsureSuccessStatusCode();

        OllamaChatResponse? result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>();
        return result?.Message?.Content ?? string.Empty;
    }

    private record OllamaMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private record OllamaChatResponse(
        [property: JsonPropertyName("message")] OllamaMessage? Message);
}