using Basket.Api.Application.Basket.GetBasketById;
using BuildingBlocks;
using Microsoft.AspNetCore.Mvc;

namespace Basket.Api.Endpoints;

public static class GetBasketByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/basket/{username}",
                async ([FromRoute] string username, [FromServices] GetBasketById handler) =>
                {
                    GetBasketByIdQuery query = new(username);
                    Result<GetBasketByIdResult> result = await handler.HandleAsync(query);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.NotFound(result.Error);
                })
            .WithName("GetBasketById")
            .WithTags("Basket")
            .Produces<GetBasketByIdResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}