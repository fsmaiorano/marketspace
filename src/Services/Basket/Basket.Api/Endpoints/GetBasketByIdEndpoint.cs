using Basket.Api.Application.Basket.GetBasketById;
using Basket.Api.Endpoints.Dto;
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

                    return result is { IsSuccess: true, Data.ShoppingCart: not null }
                        ? Results.Ok(new ShoppingCartDto
                        {
                            Username = result.Data.ShoppingCart.Username,
                            Items = result.Data.ShoppingCart.Items?
                                .Select(item => new ShoppingCartItemDto
                                {
                                    ProductId = item.ProductId, Quantity = item.Quantity, Price = item.Price
                                }).ToList() ?? []
                        })
                        : Results.BadRequest(result.Error ?? "Unknown error");
                })
            .WithName("GetBasketById")
            .WithTags("Basket")
            .Produces<GetBasketByIdResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}