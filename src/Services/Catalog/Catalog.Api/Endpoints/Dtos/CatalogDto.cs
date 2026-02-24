namespace Catalog.Api.Endpoints.Dtos;

public class CatalogDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = [];
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; } = 0.0m;
    public Guid MerchantId { get; set; } = Guid.Empty;
    public new DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}