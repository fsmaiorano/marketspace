using BuildingBlocks;
using BuildingBlocks.Pagination;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;

namespace Catalog.Api.Application.Catalog.GetCatalog;

public class GetCatalogHandler(ICatalogRepository repository, ILogger<GetCatalogHandler> logger) : IGetCatalogHandler
{
    public async Task<Result<GetCatalogResult>> HandleAsync(GetCatalogQuery query)
    {
        logger.LogInformation("Handling {Query} with {@Pagination}", nameof(GetCatalogQuery), query.Pagination);

        PaginatedResult<CatalogEntity> products = await repository.GetPaginatedListAsync(query.Pagination);

        return Result<GetCatalogResult>.Success(
            new GetCatalogResult(products.PageIndex, products.PageSize, products.Count, products.Data.ToList()));
    }
}