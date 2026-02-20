using BuildingBlocks;
using Catalog.Api.Application.Catalog.CreateCatalog;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Endpoints;

public static class CreateCatalogEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/catalog",
                async ([FromBody] CreateCatalogCommand command, [FromServices] CreateCatalog handler) =>
                {
                    Result<CreateCatalogResult> result = await handler.HandleAsync(command);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .WithName("CreateCatalog")
            .WithTags("Catalog")
            .Produces<CreateCatalogResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}