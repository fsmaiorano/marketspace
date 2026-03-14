using BuildingBlocks;
using BuildingBlocks.Loggers;
using BuildingBlocks.Pagination;
using BuildingBlocks.Storage.Minio;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Endpoints.Dtos;

namespace Catalog.Api.Application.Catalog.GetCatalogByMerchantId;

public record GetCatalogByMerchantIdQuery(Guid MerchantId, int PageIndex, int PageSize);

public record GetCatalogByMerchantIdResult(int PageIndex, int PageSize, long Count, List<CatalogDto> Products);

public class GetCatalogByMerchantId(
    ICatalogRepository repository,
    IAppLogger<GetCatalogByMerchantId> logger,
    IMinioBucket minioBucket)
{
    public async Task<Result<GetCatalogByMerchantIdResult>> HandleAsync(GetCatalogByMerchantIdQuery query)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application,
                "Processing get catalog by merchant ID request for: {MerchantId}", query.MerchantId);

            PaginationRequest pagination = new(query.PageIndex, query.PageSize);
            PaginatedResult<CatalogEntity> paginated =
                await repository.GetByMerchantIdAsync(query.MerchantId, pagination);

            List<CatalogDto> catalogDtoList = [];
            foreach (CatalogEntity product in paginated.Data)
            {
                CatalogDto catalogDto = new()
                {
                    Id = product.Id.Value,
                    Name = product.Name,
                    Categories = product.Categories,
                    Description = product.Description,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price.Value,
                    Stock = product.Stock.Available,
                    MerchantId = product.MerchantId,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                };

                string imageData = await minioBucket.GetImageAsync(product.ImageUrl) ?? string.Empty;
                catalogDto.ImageUrl = imageData;

                catalogDtoList.Add(catalogDto);
            }

            return Result<GetCatalogByMerchantIdResult>.Success(
                new GetCatalogByMerchantIdResult(paginated.PageIndex, paginated.PageSize, paginated.Count, catalogDtoList));
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while getting catalog by merchant ID.");
            return Result<GetCatalogByMerchantIdResult>.Failure("An error occurred while processing your request.");
        }
    }
}
