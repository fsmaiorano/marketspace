namespace BackendForFrontend.Api.Ai.Dtos;

public record RagRequest
{
    public string? Question { get; init; }
    public string? ContextId { get; init; }
}
