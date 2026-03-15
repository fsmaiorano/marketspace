using BackendForFrontend.Api.Ai.Dtos;
using BackendForFrontend.Api.Ai.Services;
using BuildingBlocks;
using BuildingBlocks.Loggers;

namespace BackendForFrontend.Api.Ai.UseCases;

public class AiUseCase(
    IAppLogger<AiUseCase> logger,
    AiService service)
{
    public async Task<Result<ChatResponse>> ChatAsync(ChatRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Processing chat request for user: {UserId}", request.UserId);
        return await service.ChatAsync(request);
    }

    public async Task<Result<AgentResponse>> AgentAsync(AgentRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Processing agent request for user: {UserId}", request.UserId);
        return await service.AgentAsync(request);
    }

    public async Task<Result<RagResponse>> RagAsync(RagRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Processing RAG request with contextId: {ContextId}", request.ContextId);
        return await service.RagAsync(request);
    }
}