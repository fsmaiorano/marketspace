using BuildingBlocks.Pagination;
using Catalog.Api.Domain.Entities;

namespace Catalog.Api.Application.Catalog.GetCatalog;

public record GetCatalogResult(int PageIndex, int PageSize, long Count, List<CatalogEntity> Products)
{
    public int PageIndex { get; set; } = PageIndex;
    public int PageSize { get; set; } = PageSize;
    public long Count { get; set; } = Count;
    public List<CatalogEntity> Products { get; set; } = Products;
}