using BuildingBlocks;
using BuildingBlocks.Pagination;
using Catalog.Api.Application.Catalog.GetCatalog;
using Catalog.Api.Endpoints.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Endpoints;

public static class GetCatalogEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/catalog",
                async ([AsParameters] PaginationRequest pagination, [FromServices] GetCatalog handler) =>
                {
                    GetCatalogQuery query = new(pagination);
                    Result<GetCatalogResult> result = await handler.HandleAsync(query);
                    return result is { IsSuccess: true, Data: not null }
                        ? Results.Ok(new PaginatedResult<CatalogDto>(
                            pagination.PageIndex,
                            pagination.PageSize,
                            result.Data.Count,
                            result.Data.Products.Select(p => new CatalogDto()
                            {
                                Id = p.Id,
                                Name = p.Name,
                                Description = p.Description,
                                Price = p.Price,
                                ImageUrl = p.ImageUrl,
                                Categories = p.Categories,
                                MerchantId = p.MerchantId,
                                CreatedAt = p.CreatedAt,
                                UpdatedAt = p.UpdatedAt
                            })))
                        : Results.NotFound(result.Error);
                })
            .WithName("GetCatalog")
            .WithTags("Catalog")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}