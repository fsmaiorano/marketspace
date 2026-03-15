namespace BackendForFrontend.Api.Ai.Dtos;

public record AgentRequest
{
    public string? Message { get; init; }
    public string? UserId { get; init; }
}
