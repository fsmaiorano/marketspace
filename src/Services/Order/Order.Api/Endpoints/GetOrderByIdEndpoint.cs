using BuildingBlocks;
using Order.Api.Application.Order.GetOrderById;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Order.Api.Endpoints;

public static class GetOrderByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/order/{id:guid}", async ([FromRoute] Guid id, [FromServices] IGetOrderByIdHandler handler) =>
            {
                GetOrderByIdQuery query = new(id);
                Result<GetOrderByIdResult> result = await handler.HandleAsync(query);
                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.NotFound(result.Error);
            })
            .WithName("GetOrderById")
            .WithTags("Order")
            .Produces<GetOrderByIdResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}