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
                    Result<CreateMerchantResponse> result = await usecase.CreateMerchantAsync(request);
                    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
                })
            .WithName("CreateMerchant")
            .WithTags("Merchant")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/api/merchant/{merchantId:guid}",
                async ([FromRoute] Guid merchantId, [FromServices] IMerchantUseCase usecase) =>
                {
                    Result<GetMerchantByIdResponse> result = await usecase.GetMerchantByIdAsync(merchantId);
                    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
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
                    Result<UpdateMerchantResponse> result = await usecase.UpdateMerchantAsync(request);
                    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
                })
            .WithName("UpdateMerchant")
            .WithTags("Merchant")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapDelete("/api/merchant/{merchantId:guid}",
                async ([FromRoute] Guid merchantId, [FromServices] IMerchantUseCase usecase) =>
                {
                    Result<DeleteMerchantResponse> result = await usecase.DeleteMerchantAsync(merchantId);
                    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
                })
            .WithName("DeleteMerchant")
            .WithTags("Merchant")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}