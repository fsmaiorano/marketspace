using Merchant.Api.Application.Merchant.UpdateMerchant;
using Microsoft.AspNetCore.Mvc;

namespace Merchant.Api.Endpoints;

public static class UpdateMerchantEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/merchant",
                async ([FromBody] UpdateMerchantCommand command, [FromServices] UpdateMerchant handler) =>
                {
                    Result<UpdateMerchantResult> result = await handler.HandleAsync(command);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .WithName("UpdateMerchant")
            .WithTags("Merchant")
            .Produces<UpdateMerchantResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}