using BuildingBlocks;
using Microsoft.AspNetCore.Mvc;
using Payment.Api.Application.Payment.DeletePayment;

namespace Payment.Api.Endpoints;

public static class DeletePaymentEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/payment/{id:guid}",
                async ([FromRoute] Guid id, [FromServices] IDeletePaymentHandler handler) =>
                {
                    Result<DeletePaymentResult> result = await handler.HandleAsync(new DeletePaymentCommand(id));
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .WithName("DeletePayment")
            .WithTags("Payment")
            .Produces<Result<DeletePaymentResult>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
