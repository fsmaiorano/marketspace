using BuildingBlocks;
using BuildingBlocks.Pagination;
using Catalog.Api.Application.Dtos;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;

namespace Catalog.Api.Application.Catalog.GetCatalog;

public class GetCatalogHandler(ICatalogRepository repository, ILogger<GetCatalogHandler> logger) : IGetCatalogHandler
{
    public async Task<Result<GetCatalogResult>> HandleAsync(GetCatalogQuery query)
    {
        logger.LogInformation("Handling {Query} with {@Pagination}", nameof(GetCatalogQuery), query.Pagination);

        PaginatedResult<CatalogEntity> products = await repository.GetPaginatedListAsync(query.Pagination);

        List<CatalogDto> catalogDtoList = products.Data.Select(product => new CatalogDto
        {
            Id = product.Id.Value,
            Name = product.Name,
            Categories = product.Categories,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            Price = product.Price.Value,
            MerchantId = product.MerchantId,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        }).ToList();

        return Result<GetCatalogResult>.Success(
            new GetCatalogResult(products.PageIndex, products.PageSize, products.Count, catalogDtoList.ToList()));
    }
}