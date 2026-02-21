using BuildingBlocks;
using Order.Api.Application.Order.CreateOrder;
using Microsoft.AspNetCore.Mvc;

namespace Order.Api.Endpoints;

public static class CreateOrderEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/order",
                async ([FromBody] CreateOrderCommand command, [FromServices] CreateOrder handler) =>
                {
                    Result<CreateOrderResult> result = await handler.HandleAsync(command);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .WithName("CreateOrder")
            .WithTags("Order")
            .Produces<CreateOrderResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}