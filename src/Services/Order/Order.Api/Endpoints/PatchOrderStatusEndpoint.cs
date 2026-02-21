using BuildingBlocks;
using Microsoft.AspNetCore.Mvc;
using Order.Api.Application.Order.PatchOrderStatus;

namespace Order.Api.Endpoints;

public static class PatchOrderStatusEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/order/status",
                async ([FromBody] PatchOrderStatusCommand command,
                    [FromServices] PatchOrderStatus handler) =>
                {
                    Result<PatchOrderStatusResult> result = await handler.HandleAsync(command);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .WithName("PatchOrderStatus")
            .WithTags("Order")
            .Produces<PatchOrderStatusResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}