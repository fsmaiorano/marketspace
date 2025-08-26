using BuildingBlocks;
using BuildingBlocks.Pagination;
using Catalog.Api.Application.Catalog.GetCatalog;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Endpoints;

public static class GetCatalogEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/catalog",
                async ([AsParameters] PaginationRequest pagination, [FromServices] IGetCatalogHandler handler) =>
                {
                    GetCatalogQuery query = new(pagination);
                    Result<GetCatalogResult> result = await handler.HandleAsync(query);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.NotFound(result.Error);
                })
            .WithName("GetCatalog")
            .WithTags("Catalog")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}