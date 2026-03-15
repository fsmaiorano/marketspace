namespace Ai.Api.Endpoints.Dtos;

public record ChatRequest
{
    public string? Message { get; init; }
    public string? UserId { get; init; }
}