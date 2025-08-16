using Merchant.Api.Application.Merchant.DeleteMerchant;
using Microsoft.AspNetCore.Mvc;

namespace Merchant.Api.Endpoints;

public static class DeleteMerchantEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/merchant", async ([FromBody] DeleteMerchantCommand command, [FromServices] IDeleteMerchantHandler handler) =>
            {
                Result<DeleteMerchantResult> result = await handler.HandleAsync(command);
                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.BadRequest(result.Error);
            })
            .WithName("DeleteMerchant")
            .WithTags("Merchant")
            .Produces<DeleteMerchantResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}