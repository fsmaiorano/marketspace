namespace Catalog.Api.Application.Catalog.UpdateCatalog;

public class UpdateCatalogCommand
{
    public required Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; } = 0.0m;
    public List<string> Categories { get; set; } = [];
}