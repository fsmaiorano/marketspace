using Ai.Api.Domain.Interfaces;
using Ai.Api.Endpoints.Dtos;

namespace Ai.Api.Application;

public class ChatUseCase(ILLMClient llmClient)
{
    private const string SystemPrompt =
        "You are a helpful AI assistant for MarketSpace, an e-commerce marketplace. " +
        "Answer questions about orders, products, and services politely and concisely.";

    public async Task<ChatResponse> ChatAsync(ChatRequest request)
    {
        string prompt = $"{SystemPrompt}\n\nUser: {request.Message}\nAssistant:";
        string answer = await llmClient.Generate(prompt);
        return new ChatResponse { Answer = answer.Trim() };
    }
}
