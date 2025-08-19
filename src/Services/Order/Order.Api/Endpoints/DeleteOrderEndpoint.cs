using BuildingBlocks;
using Order.Api.Application.Order.DeleteOrder;
using Microsoft.AspNetCore.Mvc;

namespace Order.Api.Endpoints;

public static class DeleteOrderEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/order",
                async ([FromBody] DeleteOrderCommand command, [FromServices] IDeleteOrderHandler handler) =>
                {
                    Result<DeleteOrderResult> result = await handler.HandleAsync(command);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .WithName("DeleteOrder")
            .WithTags("Order")
            .Produces<DeleteOrderResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}