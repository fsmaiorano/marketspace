using BuildingBlocks;
using Catalog.Api.Application.Catalog.DeleteCatalog;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Endpoints;

public static class DeleteBasketEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/catalog",
                async ([FromBody] DeleteCatalogCommand command, [FromServices] IDeleteCatalogHandler handler) =>
                {
                    Result<DeleteCatalogResult> result = await handler.HandleAsync(command);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .WithName("DeleteCatalog")
            .WithTags("Catalog")
            .Produces<DeleteCatalogResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}