using BackendForFrontend.Api.Basket.Dtos;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks;
using BackendForFrontend.Api.Basket.UseCases;
using System.Security.Claims;

namespace BackendForFrontend.Api.Basket.Endpoints;

public static class BasketEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/basket",
                async ([FromBody] CreateBasketRequest request, [FromServices] BasketUseCase usecase) =>
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
                async ([FromRoute] string username, [FromServices] BasketUseCase usecase) =>
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
                async ([FromRoute] string username, [FromServices] BasketUseCase usecase) =>
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
                async ([FromBody] CheckoutBasketRequest request, ClaimsPrincipal user, [FromServices] BasketUseCase usecase) =>
                {
                    string? customerId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                         ?? user.FindFirst("sub")?.Value;
                    if (!string.IsNullOrWhiteSpace(customerId))
                        request.CustomerId = customerId;

                    Result<CheckoutBasketResponse> result = await usecase.CheckoutBasketAsync(request);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("CheckoutBasket")
            .WithTags("Basket")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}