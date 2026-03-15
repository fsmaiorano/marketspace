using Ai.Api.Endpoints.Dtos;

namespace Ai.Api.Domain;

public interface IAiService
{
    Task<ChatResponse> Chat(ChatRequest request);

    Task<RagResponse> Ask(RagRequest request);

    Task<AgentResponse> Agent(AgentRequest request);
}