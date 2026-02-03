using BuildingBlocks;
using Microsoft.AspNetCore.Mvc;
using Payment.Api.Application.Payment.CreatePayment;

namespace Payment.Api.Endpoints;

public static class CreatePaymentEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/payment",
                async ([FromBody] CreatePaymentCommand command, [FromServices] ICreatePaymentHandler handler) =>
                {
                    Result<CreatePaymentResult> result = await handler.HandleAsync(command);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .WithName("CreatePayment")
            .WithTags("Payment")
            .Produces<Result<CreatePaymentResult>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
