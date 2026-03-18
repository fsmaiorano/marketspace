using BackendForFrontend.Api.Ai.Dtos;
using BackendForFrontend.Api.Base;
using BuildingBlocks;
using BuildingBlocks.Loggers;

namespace BackendForFrontend.Api.Ai.Services;

public class AiService(
    IAppLogger<AiService> logger,
    HttpClient httpClient,
    IConfiguration configuration)
    : BaseService(httpClient)
{
    private string BaseUrl => configuration["Services:AiService:BaseUrl"] ??
                              throw new ArgumentNullException("AiService BaseUrl is not configured");

    public async Task<Result<ChatResponse>> ChatAsync(ChatRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Sending chat message for user: {UserId}", request.UserId);

        try
        {
            HttpResponseMessage response = await DoPost($"{BaseUrl}/chat", request);

            if (response.IsSuccessStatusCode)
            {
                ChatResponse? result = await response.Content.ReadFromJsonAsync<ChatResponse>();
                if (result is not null)
                {
                    logger.LogInformation(LogTypeEnum.Business, "Chat response received successfully");
                    return Result<ChatResponse>.Success(result);
                }
                return Result<ChatResponse>.Failure("Empty response from AI service");
            }

            logger.LogError(LogTypeEnum.Application, null, "Failed to get chat response. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = await response.Content.ReadAsStringAsync();
            return Result<ChatResponse>.Failure($"AI chat error: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Application, ex, "Exception calling AI chat");
            return Result<ChatResponse>.Failure(ex.Message);
        }
    }

    public async Task<Result<AgentResponse>> AgentAsync(AgentRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Sending agent request for user: {UserId}", request.UserId);

        try
        {
            HttpResponseMessage response = await DoPost($"{BaseUrl}/agent", request);

            if (response.IsSuccessStatusCode)
            {
                AgentResponse? result = await response.Content.ReadFromJsonAsync<AgentResponse>();
                if (result is not null)
                {
                    logger.LogInformation(LogTypeEnum.Business, "Agent response received successfully");
                    return Result<AgentResponse>.Success(result);
                }
                return Result<AgentResponse>.Failure("Empty response from AI service");
            }

            logger.LogError(LogTypeEnum.Application, null, "Failed to get agent response. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = await response.Content.ReadAsStringAsync();
            return Result<AgentResponse>.Failure($"AI agent error: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Application, ex, "Exception calling AI agent");
            return Result<AgentResponse>.Failure(ex.Message);
        }
    }

    public async Task<Result<RagResponse>> RagAsync(RagRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Sending RAG request with contextId: {ContextId}", request.ContextId);

        try
        {
            HttpResponseMessage response = await DoPost($"{BaseUrl}/rag", request);

            if (response.IsSuccessStatusCode)
            {
                RagResponse? result = await response.Content.ReadFromJsonAsync<RagResponse>();
                if (result is not null)
                {
                    logger.LogInformation(LogTypeEnum.Business, "RAG response received successfully");
                    return Result<RagResponse>.Success(result);
                }
                return Result<RagResponse>.Failure("Empty response from AI service");
            }

            logger.LogError(LogTypeEnum.Application, null, "Failed to get RAG response. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = await response.Content.ReadAsStringAsync();
            return Result<RagResponse>.Failure($"AI RAG error: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Application, ex, "Exception calling AI RAG");
            return Result<RagResponse>.Failure(ex.Message);
        }
    }
}