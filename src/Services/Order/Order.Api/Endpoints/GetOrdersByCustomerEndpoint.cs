using BuildingBlocks;
using Order.Api.Application.Order.GetOrdersByCustomer;
using Order.Api.Endpoints.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Order.Api.Endpoints;

public static class GetOrdersByCustomerEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/order/customer/{customerId}",
                async ([FromRoute] Guid customerId, [FromServices] GetOrdersByCustomer handler,
                    [FromQuery] int limit = 10) =>
                {
                    GetOrdersByCustomerQuery query = new(customerId, limit);
                    Result<GetOrdersByCustomerResult> result = await handler.HandleAsync(query);

                    return result is { IsSuccess: true, Data: not null }
                        ? Results.Ok(result.Data.Orders.Select(o => new OrderDto
                        {
                            Id = o.Id.Value,
                            CustomerId = o.CustomerId.Value,
                            TotalAmount = o.TotalAmount.Value,
                            Status = o.Status.ToString(),
                            CreatedAt = o.CreatedAt!.Value,
                        }).ToList())
                        : Results.Problem(result.Error);
                })
            .WithName("GetOrdersByCustomer")
            .WithTags("Order")
            .Produces<List<OrderDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
