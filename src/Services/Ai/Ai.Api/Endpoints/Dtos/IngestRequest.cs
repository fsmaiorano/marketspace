namespace Ai.Api.Endpoints.Dtos;

public record IngestRequest
{
    public List<string> Documents { get; init; } = [];
    public string? ContextId { get; init; }
    public string? Metadata { get; init; }
}
