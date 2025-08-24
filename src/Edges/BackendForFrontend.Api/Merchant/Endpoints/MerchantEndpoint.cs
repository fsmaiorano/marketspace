using BackendForFrontend.Api.Merchant.Contracts;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks;

namespace BackendForFrontend.Api.Merchant.Endpoints;

public static class MerchantEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/merchant",
                async ([FromBody] Dtos.CreateMerchantRequest request, [FromServices] IMerchantUseCase usecase) =>
                {
                    var result = await usecase.CreateMerchantAsync(request);
                    return Results.Ok(Result<Dtos.CreateMerchantResponse>.Success(result));
                })
            .WithName("CreateMerchant")
            .WithTags("Merchant")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}