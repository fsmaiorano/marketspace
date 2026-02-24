using Basket.Api.Application.Basket.CreateBasket;
using Basket.Api.Endpoints.Dto;
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
            .WithName("CreateBasket")
            .WithTags("Basket")
            .Produces<CreateBasketResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}