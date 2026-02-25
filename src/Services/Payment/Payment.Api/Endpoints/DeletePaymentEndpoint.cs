using BuildingBlocks;
using Microsoft.AspNetCore.Mvc;
using Payment.Api.Application.Payment.DeletePayment;

namespace Payment.Api.Endpoints;

public static class DeletePaymentEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/payment/{id}",
                async ([FromRoute] string id, [FromServices] DeletePayment handler) =>
                {
                    Result<DeletePaymentResult> result =
                        await handler.HandleAsync(new DeletePaymentCommand(Guid.Parse(id)));
                    return result.IsSuccess
                        ? Results.NoContent()
                        : Results.BadRequest(result.Error);
                })
            .WithName("DeletePayment")
            .WithTags("Payment")
            .Produces<Result<DeletePaymentResult>>(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}