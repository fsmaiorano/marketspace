using BuildingBlocks;
using Catalog.Api.Application.Catalog.UpdateCatalog;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Endpoints;

public static class UpdateCatalogEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/catalog",
                async ([FromBody] UpdateCatalogCommand command, [FromServices] IUpdateCatalogHandler handler) =>
                {
                    Result<UpdateCatalogResult> result = await handler.HandleAsync(command);
                    return result.IsSuccess
                        ? Results.Ok(result.Value)
                        : Results.BadRequest(result.Error);
                })
            .WithName("UpdateCatalog")
            .WithTags("Catalog")
            .Produces<UpdateCatalogResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}