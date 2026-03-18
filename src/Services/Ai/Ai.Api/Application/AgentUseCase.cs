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
    ConversationStore conversationStore,
    BuildingBlocks.Loggers.IAppLogger<AgentUseCase> logger)
{
    [GeneratedRegex(
        @"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b",
        RegexOptions.Compiled)]
    private static partial Regex GuidRegex();

    private static readonly Regex OrderStatusKeywords = new(
        @"\border\s*(status|details?|info(rmation)?)?\b|\btrack\s*order\b" +
        @"|\bpedido\b|\bconsultar\s*pedido\b|\bstatus\s*do\s*pedido\b|\brastrear\b|\bdetalhes\s*do\s*pedido\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MyOrdersKeywords = new(
        @"\b(my\s+orders?|order\s+history|recent\s+orders?|past\s+orders?|list\s+.*orders?)\b" +
        @"|\b(meus\s+pedidos?|hist[oó]rico\s+de\s+pedidos?|pedidos?\s+recentes?|listar\s+pedidos?|ver\s+pedidos?|meus\s+compras?)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ProductKeywords = new(
        @"\b(product|products|item|items|catalog|buy|purchase|available|price|stock|sell|selling|shop|shopping|find|search|show|list)\b" +
        @"|\b(produto|produtos|cat[aá]logo|comprar|pre[cç]o|estoque|dispon[ií]vel|buscar|pesquisar|loja)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<AgentResponse> AgentAsync(AgentRequest request)
    {
        string message = request.Message ?? string.Empty;
        string sessionId = request.UserId ?? "anonymous";
        logger.LogInformation(BuildingBlocks.Loggers.LogTypeEnum.Application, "Agent request received. user:{UserId} message:{Message}", request.UserId, message);

        bool usedTools = false;
        string toolResult = string.Empty;
        string toolUsed = string.Empty;

            // Tool 1: Get specific order status by ID
            if (OrderStatusKeywords.IsMatch(message))
            {
                Match guidMatch = GuidRegex().Match(message);
                if (guidMatch.Success && Guid.TryParse(guidMatch.Value, out Guid orderId))
                {
                    try
                    {
                        logger.LogInformation(BuildingBlocks.Loggers.LogTypeEnum.Application, "Calling GetOrderStatusTool for order:{OrderId}", orderId);
                        string? status = await orderStatusTool.GetOrderStatusAsync(orderId);
                        logger.LogInformation(BuildingBlocks.Loggers.LogTypeEnum.Application, "GetOrderStatusTool returned for order:{OrderId} success:{HasData}", orderId, status is not null);
                        if (status is not null)
                        {
                            toolResult = status;
                            toolUsed = "GetOrderStatus";
                            usedTools = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(BuildingBlocks.Loggers.LogTypeEnum.Application, ex, "GetOrderStatusTool failed for order:{OrderId}", orderId);
                    }
                }
            }

            // Tool 2: List orders for the authenticated customer
            if (!usedTools && MyOrdersKeywords.IsMatch(message) && request.UserId is not null
                && Guid.TryParse(request.UserId, out Guid customerId))
            {
                try
                {
                    logger.LogInformation(BuildingBlocks.Loggers.LogTypeEnum.Application, "Calling GetOrdersByCustomerTool for customer:{CustomerId}", customerId);
                    string? orders = await ordersByCustomerTool.GetOrdersByCustomerAsync(customerId);
                    logger.LogInformation(BuildingBlocks.Loggers.LogTypeEnum.Application, "GetOrdersByCustomerTool returned for customer:{CustomerId} success:{HasData}", customerId, orders is not null);
                    if (orders is not null)
                    {
                        toolResult = orders;
                        toolUsed = "GetOrdersByCustomer";
                        usedTools = true;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(BuildingBlocks.Loggers.LogTypeEnum.Application, ex, "GetOrdersByCustomerTool failed for customer:{CustomerId}", customerId);
                }
            }

            // Tool 3: Search product catalog
            if (!usedTools && ProductKeywords.IsMatch(message))
            {
                try
                {
                    logger.LogInformation(BuildingBlocks.Loggers.LogTypeEnum.Application, "Calling SearchProductsTool");
                    string? products = await searchProductsTool.SearchAsync();
                    logger.LogInformation(BuildingBlocks.Loggers.LogTypeEnum.Application, "SearchProductsTool returned success:{HasData}", products is not null);
                    if (products is not null)
                    {
                        toolResult = products;
                        toolUsed = "SearchProducts";
                        usedTools = true;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(BuildingBlocks.Loggers.LogTypeEnum.Application, ex, "SearchProductsTool failed");
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
                You are a helpful MarketSpace assistant. Respond in the same language the user used.
                Answer the user's question.
                If they ask about a specific order, tell them to provide a valid order ID (GUID format).
                If they ask about their orders list, make sure they are logged in.
                If they ask about products, tell them they can ask you to search the catalog.
                {historyBlock}
                User: {message}
                Assistant:
                """
        };

        string answer;
        try
        {
            logger.LogInformation(BuildingBlocks.Loggers.LogTypeEnum.Application, "Calling LLM generate");

            // Add a timeout wrapper around LLM generation to avoid indefinite hangs
            Task<string> genTask = llmClient.Generate(prompt);
            Task completed = await Task.WhenAny(genTask, Task.Delay(TimeSpan.FromSeconds(30)));
            if (completed == genTask)
            {
                answer = await genTask;
                logger.LogInformation(BuildingBlocks.Loggers.LogTypeEnum.Application, "LLM generate completed. length:{Length}", answer?.Length ?? 0);
                if (string.IsNullOrWhiteSpace(answer))
                {
                    // Fall back to tool output when LLM returns empty
                    answer = usedTools ? toolResult : "";
                }
            }
            else
            {
                // Timeout
                logger.LogError(BuildingBlocks.Loggers.LogTypeEnum.Application, null, "LLM generate timed out after 30s");
                if (usedTools && !string.IsNullOrWhiteSpace(toolResult))
                {
                    answer = $"(Tool response)\n{toolResult}";
                }
                else
                {
                    answer = "Sorry, I'm having trouble generating a response right now. Please try again later.";
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(BuildingBlocks.Loggers.LogTypeEnum.Application, ex, "LLM generate failed");
            // If generation fails but we used a tool, return the tool output as a graceful fallback.
            // Otherwise, propagate a friendly message.
            if (usedTools && !string.IsNullOrWhiteSpace(toolResult))
            {
                answer = $"(Tool response)\n{toolResult}";
            }
            else
            {
                answer = "Sorry, I'm having trouble generating a response right now. Please try again later.";
            }

            // Record error in conversation memory for debugging
            conversationStore.AddMessage(sessionId, "assistant", $"[ERROR] {ex.Message}");
        }

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

