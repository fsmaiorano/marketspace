using BackendForFrontend.Api.Merchant.Contracts;
using BackendForFrontend.Api.Merchant.Dtos;
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
                    CreateMerchantResponse result = await usecase.CreateMerchantAsync(request);
                    return Results.Ok(Result<Dtos.CreateMerchantResponse>.Success(result));
                })
            .WithName("CreateMerchant")
            .WithTags("Merchant")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/api/merchant/{merchantId:guid}",
                async ([FromRoute] Guid merchantId, [FromServices] IMerchantUseCase usecase) =>
                {
                    GetMerchantByIdResponse result = await usecase.GetMerchantByIdAsync(merchantId);
                    return Results.Ok(Result<Dtos.GetMerchantByIdResponse>.Success(result));
                })
            .WithName("GetMerchantById")
            .WithTags("Merchant")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapPut("/api/merchant/{merchantId:guid}",
                async ([FromRoute] Guid merchantId, [FromBody] Dtos.UpdateMerchantRequest request,
                    [FromServices] IMerchantUseCase usecase) =>
                {
                    request.Id = merchantId;
                    UpdateMerchantResponse result = await usecase.UpdateMerchantAsync(request);
                    return Results.Ok(Result<Dtos.UpdateMerchantResponse>.Success(result));
                })
            .WithName("UpdateMerchant")
            .WithTags("Merchant")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapDelete("/api/merchant/{merchantId:guid}",
                async ([FromRoute] Guid merchantId, [FromServices] IMerchantUseCase usecase) =>
                {
                    await usecase.DeleteMerchantAsync(merchantId);
                    return Results.Ok();
                })
            .WithName("DeleteMerchant")
            .WithTags("Merchant")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}