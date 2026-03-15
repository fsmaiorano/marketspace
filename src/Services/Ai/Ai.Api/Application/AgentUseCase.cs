using System.Text.RegularExpressions;
using Ai.Api.Application.Tools;
using Ai.Api.Domain.Interfaces;
using Ai.Api.Endpoints.Dtos;

namespace Ai.Api.Application;

public partial class AgentUseCase(ILLMClient llmClient, GetOrderStatusTool orderStatusTool)
{
    [GeneratedRegex(
        @"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b",
        RegexOptions.Compiled)]
    private static partial Regex GuidRegex();

    private static readonly Regex OrderKeywords = new(
        @"\border\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<AgentResponse> AgentAsync(AgentRequest request)
    {
        string message = request.Message ?? string.Empty;
        bool usedTools = false;
        string toolResult = string.Empty;

        if (OrderKeywords.IsMatch(message))
        {
            Match guidMatch = GuidRegex().Match(message);
            if (guidMatch.Success && Guid.TryParse(guidMatch.Value, out Guid orderId))
            {
                string? status = await orderStatusTool.GetOrderStatusAsync(orderId);
                if (status is not null)
                {
                    toolResult = status;
                    usedTools = true;
                }
            }
        }

        string prompt = usedTools
            ? $"""
               You are a helpful MarketSpace assistant. Answer the user's question using the order information below.

               Order information: {toolResult}

               User: {message}
               Assistant:
               """
            : $"""
               You are a helpful MarketSpace assistant. Answer the user's question.
               If they ask about a specific order status, tell them they need to provide a valid order ID.

               User: {message}
               Assistant:
               """;

        string answer = await llmClient.Generate(prompt);

        return new AgentResponse
        {
            Answer = answer.Trim(),
            UsedTools = usedTools
        };
    }
}
