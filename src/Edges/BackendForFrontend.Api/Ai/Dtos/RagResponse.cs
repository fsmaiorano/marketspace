namespace BackendForFrontend.Api.Ai.Dtos;

public record RagResponse
{
    public string? Answer { get; init; }
    public ICollection<string> Sources { get; init; } = [];
}
