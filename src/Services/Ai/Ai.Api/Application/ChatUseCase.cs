using Ai.Api.Domain.Interfaces;
using Ai.Api.Endpoints.Dtos;
using BuildingBlocks.Loggers;

namespace Ai.Api.Application;

public class ChatUseCase(ILLMClient llmClient, IAppLogger<ChatUseCase> logger)
{
    private const string SystemPrompt =
        "You are a helpful AI assistant for MarketSpace, an e-commerce marketplace. " +
        "Answer questions about orders, products, and services politely and concisely.";

    public async Task<ChatResponse> ChatAsync(ChatRequest request)
    {
        string prompt = $"{SystemPrompt}\n\nUser: {request.Message}\nAssistant:";
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Calling LLM for chat. user:{UserId}", request.UserId);
            string answer = await llmClient.Generate(prompt);
            logger.LogInformation(LogTypeEnum.Application, "LLM chat response length:{Length}", answer?.Length ?? 0);
            if (string.IsNullOrWhiteSpace(answer))
                return new ChatResponse { Answer = "I could not generate a response. Please try again." };
            return new ChatResponse { Answer = answer.Trim() };
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Application, ex, "LLM chat generation failed");
            return new ChatResponse { Answer = "I am having trouble responding right now. Please try again in a moment." };
        }
    }
}
