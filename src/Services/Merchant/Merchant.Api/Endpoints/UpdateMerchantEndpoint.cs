using Merchant.Api.Application.Merchant.UpdateMerchant;

namespace Merchant.Api.Endpoints;

public static class UpdateMerchantEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/merchant",
                async (UpdateMerchantCommand command, IUpdateMerchantHandler handler) =>
                {
                    Result<UpdateMerchantResult> result = await handler.HandleAsync(command);
                    return result.IsSuccess
                        ? Results.Ok(result.Value)
                        : Results.BadRequest(result.Error);
                })
            .WithName("UpdateMerchant")
            .WithTags("Merchant")
            .Produces<UpdateMerchantResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}