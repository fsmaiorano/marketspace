using BuildingBlocks;
using Order.Api.Application.Order.GetOrderById;
using Microsoft.AspNetCore.Mvc;
using Order.Api.Endpoints.Dto;

namespace Order.Api.Endpoints;

public static class GetOrderByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/order/{id}", async ([FromRoute] string id, [FromServices] GetOrderById handler) =>
            {
                GetOrderByIdQuery query = new(Guid.Parse(id));
                Result<GetOrderByIdResult> result = await handler.HandleAsync(query);
                return result is { IsSuccess: true, Data: not null }
                    ? Results.Ok(new OrderDto()
                    {
                        Id = result.Data.Order.Id.Value,
                        CustomerId = result.Data.Order.CustomerId.Value,
                        TotalAmount = result.Data.Order.TotalAmount.Value,
                        Status = result.Data.Order.Status.ToString(),
                        CreatedAt = result.Data.Order.CreatedAt!.Value,
                    })
                    : Results.NotFound(result.Error);
            })
            .WithName("GetOrderById")
            .WithTags("Order")
            .Produces<GetOrderByIdResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}