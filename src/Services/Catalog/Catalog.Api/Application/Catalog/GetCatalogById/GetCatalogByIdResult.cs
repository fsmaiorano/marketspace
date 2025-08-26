using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Catalog.Api.Application.Catalog.GetCatalogById;

public class GetCatalogByIdResult
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public decimal Price { get; init; } = 0.0m;
    
    [JsonPropertyName("categories")]
    public IReadOnlyList<string> Categories { get; init; } = [];
}