using BuildingBlocks.Pagination;
using Catalog.Api.Domain.Entities;

namespace Catalog.Api.Application.Catalog.GetCatalog;

public record GetCatalogResult
{
    public List<CatalogEntity> Products { get; init; } = null!;
}