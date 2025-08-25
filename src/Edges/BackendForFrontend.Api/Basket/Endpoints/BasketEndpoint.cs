using BackendForFrontend.Api.Basket.Contracts;
using BackendForFrontend.Api.Basket.Dtos;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks;

namespace BackendForFrontend.Api.Basket.Endpoints;

public static class BasketEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/basket",
                async ([FromBody] CreateBasketRequest request, [FromServices] IBasketUseCase usecase) =>
                {
                    CreateBasketResponse result = await usecase.CreateBasketAsync(request);
                    return Results.Ok(Result<CreateBasketResponse>.Success(result));
                })
            .WithName("CreateBasket")
            .WithTags("Basket")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/api/basket/{username}",
                async ([FromRoute] string username, [FromServices] IBasketUseCase usecase) =>
                {
                    GetBasketResponse result = await usecase.GetBasketByIdAsync(username);
                    return Results.Ok(Result<GetBasketResponse>.Success(result));
                })
            .WithName("GetBasketByUsername")
            .WithTags("Basket")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapDelete("/api/basket/{username}",
                async ([FromRoute] string username, [FromServices] IBasketUseCase usecase) =>
                {
                    DeleteBasketResponse result = await usecase.DeleteBasketAsync(username);
                    return Results.Ok(Result<DeleteBasketResponse>.Success(result));
                })
            .WithName("DeleteBasket")
            .WithTags("Basket")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapPost("/api/basket/checkout",
                async ([FromBody] CheckoutBasketRequest request, [FromServices] IBasketUseCase usecase) =>
                {
                    CheckoutBasketResponse result = await usecase.CheckoutBasketAsync(request);
                    return Results.Ok(Result<CheckoutBasketResponse>.Success(result));
                })
            .WithName("CheckoutBasket")
            .WithTags("Basket")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
