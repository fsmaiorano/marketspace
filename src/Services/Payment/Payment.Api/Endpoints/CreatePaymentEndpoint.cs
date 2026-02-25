using BuildingBlocks;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using Payment.Api.Application.Payment.CreatePayment;

namespace Payment.Api.Endpoints;

public static class CreatePaymentEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/payment",
                async ([FromBody] CreatePaymentCommand command, [FromServices] CreatePayment handler) =>
                {
                    Result<CreatePaymentResult> result = await handler.HandleAsync(command);
                    return result is { IsSuccess: true, Data: not null }
                        ? Results.Created()
                        : Results.BadRequest(result.Error);
                })
            .WithName("CreatePayment")
            .WithTags("Payment")
            .Produces<Result<CreatePaymentResult>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}