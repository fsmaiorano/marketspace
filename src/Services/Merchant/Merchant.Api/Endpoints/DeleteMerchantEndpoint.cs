using Merchant.Api.Application.Merchant.DeleteMerchant;
using Microsoft.AspNetCore.Mvc;

namespace Merchant.Api.Endpoints;

public static class DeleteMerchantEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/merchant/{merchantId}",
                async ([FromRoute] string merchantId, [FromServices] DeleteMerchant handler) =>
                {
                    DeleteMerchantCommand command = new(Guid.Parse(merchantId));
                    Result<DeleteMerchantResult> result = await handler.HandleAsync(command);
                    return result is { IsSuccess: true, Data: not null }
                        ? Results.NoContent()
                        : Results.BadRequest(result.Error);
                })
            .WithName("DeleteMerchant")
            .WithTags("Merchant")
            .Produces<DeleteMerchantResult>(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}