using BuildingBlocks;
using BuildingBlocks.Pagination;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;

namespace Catalog.Api.Application.Catalog.GetCatalog;

public class GetCatalogHandler(ICatalogRepository repository, ILogger<GetCatalogHandler> logger) : IGetCatalogHandler
{
    public async Task<Result<PaginatedResult<GetCatalogResult>>> HandleAsync(GetCatalogQuery query)
    {
        logger.LogInformation("Handling {Query} with {@Pagination}", nameof(GetCatalogQuery), query.Pagination);

        PaginatedResult<CatalogEntity> products = await repository.GetPaginatedListAsync(query.Pagination);

        products.Data.ToList()
            .ForEach(p => logger.LogInformation("Product: {ProductId} - {ProductName}", p.Id, p.Name));

        return Result<PaginatedResult<GetCatalogResult>>.Success(
            new PaginatedResult<GetCatalogResult>(
                products.PageIndex,
                products.PageSize,
                products.Count,
                new List<GetCatalogResult> { new() { Products = products.Data.ToList() } }));
    }
}