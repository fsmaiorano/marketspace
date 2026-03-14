using BuildingBlocks;
using Catalog.Api.Application.Catalog.UpdateStock;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Endpoints;

public static class UpdateStockEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/catalog/{catalogId:guid}/stock",
                async ([FromRoute] Guid catalogId, [FromBody] UpdateStockRequest request,
                    [FromServices] UpdateStock handler) =>
                {
                    UpdateStockCommand command = new(catalogId, request.Delta);
                    Result<UpdateStockResult> result = await handler.HandleAsync(command);
                    return result is { IsSuccess: true, Data: not null }
                        ? Results.Ok(new { available = result.Data.Available, reserved = result.Data.Reserved })
                        : Results.BadRequest(result.Error);
                })
            .WithName("UpdateCatalogStock")
            .WithTags("Catalog")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}

public class UpdateStockRequest
{
    public int Delta { get; set; }
}
