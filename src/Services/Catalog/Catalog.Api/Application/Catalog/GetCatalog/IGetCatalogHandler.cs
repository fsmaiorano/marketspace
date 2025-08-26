using BuildingBlocks;
using BuildingBlocks.Pagination;

namespace Catalog.Api.Application.Catalog.GetCatalog;

public interface IGetCatalogHandler
{
    Task<Result<PaginatedResult<GetCatalogResult>>> HandleAsync(GetCatalogQuery query);
}