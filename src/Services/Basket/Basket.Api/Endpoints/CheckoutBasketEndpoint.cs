using Basket.Api.Application.Basket.CheckoutBasket;
using BuildingBlocks;
using Microsoft.AspNetCore.Mvc;

namespace Basket.Api.Endpoints;

public static class CheckoutBasketEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket/checkout",
                async ([FromBody] CheckoutBasketCommand command, [FromServices] ICheckoutBasketHandler handler) =>
                {
                    Result<CheckoutBasketResult> result = await handler.HandleAsync(command);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .WithName("CheckoutBasket")
            .WithTags("Basket")
            .Produces<CheckoutBasketResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}