using BuildingBlocks;
using BuildingBlocks.Loggers;
using BuildingBlocks.Pagination;
using BuildingBlocks.Storage.Minio;
using Catalog.Api.Application.Dtos;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;

namespace Catalog.Api.Application.Catalog.GetCatalog;

public record GetCatalogQuery(PaginationRequest Pagination);

public record GetCatalogResult(int PageIndex, int PageSize, long Count, List<CatalogDto> Products);

public class GetCatalog(
    ICatalogRepository repository,
    IAppLogger<GetCatalog> logger,
    IMinioBucket minioBucket) 
{
    public async Task<Result<GetCatalogResult>> HandleAsync(GetCatalogQuery query)
    {
        logger.LogInformation(LogTypeEnum.Application, "Handling {Query} with {@Pagination}", nameof(GetCatalogQuery), query.Pagination);

        PaginatedResult<CatalogEntity> products = await repository.GetPaginatedListAsync(query.Pagination);

        List<CatalogDto> catalogDtoList = [];
        foreach (CatalogEntity product in products.Data)
        {
            CatalogDto catalogDto = new()

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
            };

            string imageData = await minioBucket.GetImageAsync(product.ImageUrl) ?? string.Empty;
            catalogDto.ImageUrl = imageData;

            catalogDtoList.Add(catalogDto);
        }

        return Result<GetCatalogResult>.Success(
            new GetCatalogResult(products.PageIndex, products.PageSize, products.Count, catalogDtoList.ToList()));
    }
}