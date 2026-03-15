namespace Ai.Api.Endpoints.Dtos;

public record AgentResponse
{
    public string? Answer { get; init; }
    public bool UsedTools { get; init; }
}