using Basket.Api.Application.Basket.CreateBasket;
using BuildingBlocks;
using Microsoft.AspNetCore.Mvc;

namespace Basket.Api.Endpoints;

public static class CreateBasketEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket",
                async ([FromBody] CreateBasketCommand command, [FromServices] CreateBasket handler) =>
                {
                    Result<CreateBasketResult> result = await handler.HandleAsync(command);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .WithName("CreateBasket")
            .WithTags("Basket")
            .Produces<CreateBasketResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}