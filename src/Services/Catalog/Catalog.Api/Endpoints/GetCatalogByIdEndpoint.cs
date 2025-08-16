using BuildingBlocks;
using Catalog.Api.Application.Catalog.GetCatalogById;
using Merchant.Api.Application.Merchant.GetMerchantById;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Catalog.Api.Endpoints;

public static class GetCatalogByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/catalog/{id:guid}", async ([FromRoute] Guid id, [FromServices] IGetCatalogByIdHandler handler) =>
            {
                GetCatalogByIdQuery query = new(id);
                Result<GetCatalogByIdResult> result = await handler.HandleAsync(query);
                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.NotFound(result.Error);
            })
            .WithName("GetCatalogById")
            .WithTags("Catalog")
            .Produces<GetCatalogByIdResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}