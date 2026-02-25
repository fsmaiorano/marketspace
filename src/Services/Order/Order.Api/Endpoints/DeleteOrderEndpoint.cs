using BuildingBlocks;
using Order.Api.Application.Order.DeleteOrder;
using Microsoft.AspNetCore.Mvc;

namespace Order.Api.Endpoints;

public static class DeleteOrderEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/order/{id}",
                async ([FromRoute] string id, [FromServices] DeleteOrder handler) =>
                {
                    DeleteOrderCommand command = new(Guid.Parse(id));
                    Result<DeleteOrderResult> result = await handler.HandleAsync(command);
                    return result is { IsSuccess: true, Data: not null }
                        ? Results.NoContent()
                        : Results.BadRequest(result.Error ?? "Unknown error");
                })
            .WithName("DeleteOrder")
            .WithTags("Order")
            .Produces<DeleteOrderResult>(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}