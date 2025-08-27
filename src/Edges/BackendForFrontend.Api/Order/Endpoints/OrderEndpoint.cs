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
                    Result<CreateOrderResponse> result = await usecase.CreateOrderAsync(request);
                    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
                })
            .WithName("CreateOrder")
            .WithTags("Order")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/api/order/{orderId:guid}",
                async ([FromRoute] Guid orderId, [FromServices] IOrderUseCase usecase) =>
                {
                    Result<GetOrderResponse> result = await usecase.GetOrderByIdAsync(orderId);
                    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
                })
            .WithName("GetOrderById")
            .WithTags("Order")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/api/order/customer/{customerId:guid}",
                async ([FromRoute] Guid customerId, [FromServices] IOrderUseCase usecase) =>
                {
                    Result<GetOrderListResponse> result = await usecase.GetOrdersByCustomerIdAsync(customerId);
                    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
                })
            .WithName("GetOrdersByCustomerId")
            .WithTags("Order")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapPut("/api/order/{orderId:guid}",
                async ([FromRoute] Guid orderId, [FromBody] UpdateOrderRequest request,
                    [FromServices] IOrderUseCase usecase) =>
                {
                    request.Id = orderId;
                    Result<UpdateOrderResponse> result = await usecase.UpdateOrderAsync(request);
                    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
                })
            .WithName("UpdateOrder")
            .WithTags("Order")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapDelete("/api/order/{orderId:guid}",
                async ([FromRoute] Guid orderId, [FromServices] IOrderUseCase usecase) =>
                {
                    Result<DeleteOrderResponse> result = await usecase.DeleteOrderAsync(orderId);
                    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
                })
            .WithName("DeleteOrder")
            .WithTags("Order")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}