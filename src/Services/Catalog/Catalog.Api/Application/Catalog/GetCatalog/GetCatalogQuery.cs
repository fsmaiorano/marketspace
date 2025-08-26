using BuildingBlocks.Pagination;

namespace Catalog.Api.Application.Catalog.GetCatalog;

public record GetCatalogQuery(PaginationRequest Pagination) 
{
    public PaginationRequest Pagination { get; init; } = Pagination;
}
