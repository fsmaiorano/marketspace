using System.Text;
using System.Text.RegularExpressions;
using Ai.Api.Application.Tools;
using Ai.Api.Domain;
using Ai.Api.Domain.Interfaces;
using Ai.Api.Endpoints.Dtos;

namespace Ai.Api.Application;

public partial class AgentUseCase(
    ILLMClient llmClient,
    GetOrderStatusTool orderStatusTool,
    GetOrdersByCustomerTool ordersByCustomerTool,
    SearchProductsTool searchProductsTool,
    ConversationStore conversationStore)
{
    [GeneratedRegex(
        @"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b",
        RegexOptions.Compiled)]
    private static partial Regex GuidRegex();

    private static readonly Regex OrderStatusKeywords = new(
        @"\border\s*(status|details?|info(rmation)?)?\b|\btrack\s*order\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MyOrdersKeywords = new(
        @"\b(my\s+orders?|order\s+history|recent\s+orders?|past\s+orders?|list\s+.*orders?)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ProductKeywords = new(
        @"\b(product|products|item|items|catalog|buy|purchase|available|price|stock|sell|selling|shop|shopping|find|search|show|list)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<AgentResponse> AgentAsync(AgentRequest request)
    {
        string message = request.Message ?? string.Empty;
        string sessionId = request.UserId ?? "anonymous";
        bool usedTools = false;
        string toolResult = string.Empty;
        string toolUsed = string.Empty;

        // Tool 1: Get specific order status by ID
        if (OrderStatusKeywords.IsMatch(message))
        {
            Match guidMatch = GuidRegex().Match(message);
            if (guidMatch.Success && Guid.TryParse(guidMatch.Value, out Guid orderId))
            {
                string? status = await orderStatusTool.GetOrderStatusAsync(orderId);
                if (status is not null)
                {
                    toolResult = status;
                    toolUsed = "GetOrderStatus";
                    usedTools = true;
                }
            }
        }

        // Tool 2: List orders for the authenticated customer
        if (!usedTools && MyOrdersKeywords.IsMatch(message) && request.UserId is not null
            && Guid.TryParse(request.UserId, out Guid customerId))
        {
            string? orders = await ordersByCustomerTool.GetOrdersByCustomerAsync(customerId);
            if (orders is not null)
            {
                toolResult = orders;
                toolUsed = "GetOrdersByCustomer";
                usedTools = true;
            }
        }

        // Tool 3: Search product catalog
        if (!usedTools && ProductKeywords.IsMatch(message))
        {
            string? products = await searchProductsTool.SearchAsync();
            if (products is not null)
            {
                toolResult = products;
                toolUsed = "SearchProducts";
                usedTools = true;
            }
        }

        // Build conversation history prefix
        string historyBlock = BuildHistoryBlock(sessionId);

        string prompt = (usedTools, toolUsed) switch
        {
            (true, "GetOrderStatus") => $"""
                You are a helpful MarketSpace assistant. Answer the user's question using the order information below.
                {historyBlock}
                Order information: {toolResult}

                User: {message}
                Assistant:
                """,

            (true, "GetOrdersByCustomer") => $"""
                You are a helpful MarketSpace assistant. The user wants to see their orders.
                Present the order list below in a friendly, readable format.
                {historyBlock}
                {toolResult}

                User: {message}
                Assistant:
                """,

            (true, "SearchProducts") => $"""
                You are a helpful MarketSpace assistant. The user is asking about products.
                Use the catalog below to answer their question. Only mention products relevant to what they asked.
                If no product matches, say so honestly.
                {historyBlock}
                Available products:
                {toolResult}

                User: {message}
                Assistant:
                """,

            _ => $"""
                You are a helpful MarketSpace assistant. Answer the user's question.
                If they ask about a specific order, tell them to provide a valid order ID.
                If they ask about their orders, make sure they are logged in (userId must be provided).
                If they ask about products, tell them they can ask you to search the catalog.
                {historyBlock}
                User: {message}
                Assistant:
                """
        };

        string answer = await llmClient.Generate(prompt);

        // Persist turn in conversation memory
        conversationStore.AddMessage(sessionId, "user", message);
        conversationStore.AddMessage(sessionId, "assistant", answer.Trim());

        return new AgentResponse
        {
            Answer = answer.Trim(),
            UsedTools = usedTools
        };
    }

    private string BuildHistoryBlock(string sessionId)
    {
        IReadOnlyList<ConversationMessage> history = conversationStore.GetHistory(sessionId);
        if (history.Count == 0)
            return string.Empty;

        StringBuilder sb = new();
        sb.AppendLine("\nPrevious conversation:");
        foreach (ConversationMessage msg in history)
            sb.AppendLine($"{(msg.Role == "user" ? "User" : "Assistant")}: {msg.Content}");
        sb.AppendLine();
        return sb.ToString();
    }
}

