using BackendForFrontend.Api.Basket.Dtos;
using BackendForFrontend.Api.Basket.UseCases;
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
                    Result<CreateBasketResponse> result = await usecase.CreateBasketAsync(request);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.NotFound(result.Error);
                })
            .RequireAuthorization()
            .WithName("CreateBasket")
            .WithTags("Basket")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/api/basket/{username}",
                async ([FromRoute] string username, [FromServices] IBasketUseCase usecase) =>
                {
                    Result<GetBasketResponse> result = await usecase.GetBasketByIdAsync(username);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.NotFound(result.Error);
                })
            .RequireAuthorization()
            .WithName("GetBasketByUsername")
            .WithTags("Basket")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapDelete("/api/basket/{username}",
                async ([FromRoute] string username, [FromServices] IBasketUseCase usecase) =>
                {
                    Result<DeleteBasketResponse> result = await usecase.DeleteBasketAsync(username);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.NotFound(result.Error);
                })
            .RequireAuthorization()
            .WithName("DeleteBasket")
            .WithTags("Basket")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapPost("/api/basket/checkout",
                async ([FromBody] CheckoutBasketRequest request, [FromServices] IBasketUseCase usecase) =>
                {
                    Result<CheckoutBasketResponse> result = await usecase.CheckoutBasketAsync(request);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.NotFound(result.Error);
                })
            .RequireAuthorization()
            .WithName("CheckoutBasket")
            .WithTags("Basket")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}