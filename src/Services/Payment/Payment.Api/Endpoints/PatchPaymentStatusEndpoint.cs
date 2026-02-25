using BuildingBlocks;
using Microsoft.AspNetCore.Mvc;
using Payment.Api.Application.Payment.PatchPaymentStatus;

namespace Payment.Api.Endpoints;

public static class PatchPaymentStatusEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/payment/status",
                async ([FromBody] PatchPaymentStatusCommand command,
                    [FromServices] PatchPaymentStatus handler) =>
                {
                    Result<PatchPaymentStatusResult> result = await handler.HandleAsync(command);
                    return result is { IsSuccess: true, Data: not null }
                        ? Results.NoContent()
                        : Results.BadRequest(result.Error);
                })
            .WithName("PatchPaymentStatus")
            .WithTags("Payment")
            .Produces<PatchPaymentStatusResult>(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}