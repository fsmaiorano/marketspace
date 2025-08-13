using BuildingBlocks;
using Merchant.Api.Application.Merchant.CreateMerchant;

namespace Merchant.Api.Endpoints;

public static class CreateMerchantEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/merchant", async (CreateMerchantCommand command, ICreateMerchantHandler handler) =>
            {
                Result<CreateMerchantResult> result = await handler.HandleAsync(command);
                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            })
            .WithName("CreateMerchant")
            .WithTags("Merchant")
            .Produces<CreateMerchantResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}