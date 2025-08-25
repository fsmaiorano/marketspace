using BackendForFrontend.Api.Order.Contracts;
using BackendForFrontend.Api.Order.Dtos;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks;

namespace BackendForFrontend.Api.Order.Endpoints;

public static class OrderEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/order",
                async ([FromBody] CreateOrderRequest request, [FromServices] IOrderUseCase usecase) =>
                {
                    CreateOrderResponse result = await usecase.CreateOrderAsync(request);
                    return Results.Ok(Result<CreateOrderResponse>.Success(result));
                })
            .WithName("CreateOrder")
            .WithTags("Order")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/api/order/{orderId:guid}",
                async ([FromRoute] Guid orderId, [FromServices] IOrderUseCase usecase) =>
                {
                    GetOrderResponse result = await usecase.GetOrderByIdAsync(orderId);
                    return Results.Ok(Result<GetOrderResponse>.Success(result));
                })
            .WithName("GetOrderById")
            .WithTags("Order")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/api/order/customer/{customerId:guid}",
                async ([FromRoute] Guid customerId, [FromServices] IOrderUseCase usecase) =>
                {
                    GetOrderListResponse result = await usecase.GetOrdersByCustomerIdAsync(customerId);
                    return Results.Ok(Result<GetOrderListResponse>.Success(result));
                })
            .WithName("GetOrdersByCustomerId")
            .WithTags("Order")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapPut("/api/order/{orderId:guid}",
                async ([FromRoute] Guid orderId, [FromBody] UpdateOrderRequest request, [FromServices] IOrderUseCase usecase) =>
                {
                    request.Id = orderId;
                    UpdateOrderResponse result = await usecase.UpdateOrderAsync(request);
                    return Results.Ok(Result<UpdateOrderResponse>.Success(result));
                })
            .WithName("UpdateOrder")
            .WithTags("Order")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapDelete("/api/order/{orderId:guid}",
                async ([FromRoute] Guid orderId, [FromServices] IOrderUseCase usecase) =>
                {
                    DeleteOrderResponse result = await usecase.DeleteOrderAsync(orderId);
                    return Results.Ok(Result<DeleteOrderResponse>.Success(result));
                })
            .WithName("DeleteOrder")
            .WithTags("Order")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
