using BuildingBlocks;
using Microsoft.AspNetCore.Mvc;
using Payment.Api.Application.Payment.UpdatePayment;

namespace Payment.Api.Endpoints;

public static class UpdatePaymentEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/payment",
                async ([FromBody] UpdatePaymentCommand command, [FromServices] UpdatePayment handler) =>
                {
                    Result<UpdatePaymentResult> result = await handler.HandleAsync(command);
                    return result is { IsSuccess: true, Data: not null }
                        ? Results.Ok()
                        : Results.BadRequest(result.Error);
                })
            .WithName("UpdatePayment")
            .WithTags("Payment")
            .Produces<Result<UpdatePaymentResult>>(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}