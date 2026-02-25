using Merchant.Api.Application.Merchant.CreateMerchant;
using Microsoft.AspNetCore.Mvc;

namespace Merchant.Api.Endpoints;

public static class CreateMerchantEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/merchant", async ([FromBody] CreateMerchantCommand command, [FromServices] CreateMerchant handler) =>
            {
                Result<CreateMerchantResult> result = await handler.HandleAsync(command);
                return result is { IsSuccess: true, Data: not null }
                    ? Results.Created()
                    : Results.BadRequest(result.Error);
            })
            .WithName("CreateMerchant")
            .WithTags("Merchant")
            .Produces<CreateMerchantResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}