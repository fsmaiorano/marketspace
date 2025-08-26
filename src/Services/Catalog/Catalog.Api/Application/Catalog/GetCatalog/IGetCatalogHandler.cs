using BuildingBlocks;
using BuildingBlocks.Pagination;

namespace Catalog.Api.Application.Catalog.GetCatalog;

public interface IGetCatalogHandler
{
    Task<Result<GetCatalogResult>> HandleAsync(GetCatalogQuery query);
}