using BuildingBlocks;
using BuildingBlocks.Pagination;
using Catalog.Api.Application.Catalog.GetCatalogByMerchantId;
using Catalog.Api.Endpoints.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Endpoints;

public static class GetCatalogByMerchantIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/catalog/merchant/{merchantId:guid}",
                async ([FromRoute] Guid merchantId, [AsParameters] PaginationRequest pagination,
                    [FromServices] GetCatalogByMerchantId handler) =>
                {
                    GetCatalogByMerchantIdQuery query = new(merchantId, pagination.PageIndex, pagination.PageSize);
                    Result<GetCatalogByMerchantIdResult> result = await handler.HandleAsync(query);
                    return result is { IsSuccess: true, Data: not null }
                        ? Results.Ok(new PaginatedResult<CatalogDto>(
                            result.Data.PageIndex,
                            result.Data.PageSize,
                            result.Data.Count,
                            result.Data.Products))
                        : Results.NotFound(result.Error);
                })
            .WithName("GetCatalogByMerchantId")
            .WithTags("Catalog")
            .Produces<PaginatedResult<CatalogDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
