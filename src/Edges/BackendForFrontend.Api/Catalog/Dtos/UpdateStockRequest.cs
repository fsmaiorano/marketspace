namespace BackendForFrontend.Api.Catalog.Dtos;

public class UpdateStockRequest
{
    public Guid CatalogId { get; set; }
    public int Delta { get; set; }
}
