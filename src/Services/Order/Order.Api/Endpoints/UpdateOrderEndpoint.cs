using BuildingBlocks;
using Order.Api.Application.Order.UpdateOrder;
using Microsoft.AspNetCore.Mvc;

namespace Order.Api.Endpoints;

public static class UpdateOrderEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/order",
                async ([FromBody] UpdateOrderCommand command, [FromServices] IUpdateOrderHandler handler) =>
                {
                    Result<UpdateOrderResult> result = await handler.HandleAsync(command);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .WithName("UpdateOrder")
            .WithTags("Order")
            .Produces<UpdateOrderResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}