namespace BackendForFrontend.Api.Catalog.Dtos;

public class UpdateCatalogResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public List<string> Categories { get; set; } = [];
    public string ImageUrl { get; set; } = string.Empty;
    public Guid MerchantId { get; set; }
}
