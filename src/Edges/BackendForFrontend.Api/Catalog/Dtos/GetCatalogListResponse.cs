namespace BackendForFrontend.Api.Catalog.Dtos;

public class GetCatalogListResponse
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public long Count { get; set; }
    public required List<CatalogDto> Products { get; set; }
}