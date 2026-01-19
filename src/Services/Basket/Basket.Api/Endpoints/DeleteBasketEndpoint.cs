using Basket.Api.Application.Basket.DeleteBasket;
using BuildingBlocks;
using Microsoft.AspNetCore.Mvc;

namespace Basket.Api.Endpoints;

public static class DeleteBasketEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/basket/{username}",
                async ([FromRoute] string username, [FromServices] IDeleteBasketHandler handler) =>
                {
                    DeleteBasketCommand command = new() { Username = username };
                    Result<DeleteBasketResult> result = await handler.HandleAsync(command);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .WithName("DeleteBasket")
            .WithTags("Basket")
            .Produces<DeleteBasketResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}