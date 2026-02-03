using BuildingBlocks;
using Microsoft.AspNetCore.Mvc;
using Payment.Api.Application.Payment.GetPaymentById;

namespace Payment.Api.Endpoints;

public static class GetPaymentByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/payment/{id:guid}",
                async ([FromRoute] Guid id, [FromServices] IGetPaymentByIdHandler handler) =>
                {
                    Result<GetPaymentByIdResult> result = await handler.HandleAsync(new GetPaymentByIdQuery(id));
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.NotFound(result.Error);
                })
            .WithName("GetPaymentById")
            .WithTags("Payment")
            .Produces<Result<GetPaymentByIdResult>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
