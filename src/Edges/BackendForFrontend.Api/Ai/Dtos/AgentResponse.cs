namespace BackendForFrontend.Api.Ai.Dtos;

public record AgentResponse
{
    public string? Answer { get; init; }
    public bool UsedTools { get; init; }
}
