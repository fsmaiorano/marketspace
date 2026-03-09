using BackendForFrontend.Api.Merchant.Dtos;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks;
using BackendForFrontend.Api.Merchant.UseCases;
using System.Security.Claims;

namespace BackendForFrontend.Api.Merchant.Endpoints;

public static class MerchantEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/merchant",
                async ([FromBody] CreateMerchantRequest request, [FromServices] MerchantUseCase usecase) =>
                {
                    Result<CreateMerchantResponse> result = await usecase.CreateMerchantAsync(request);
                    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("CreateMerchant")
            .WithTags("Merchant")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/api/merchant/me",
                async (ClaimsPrincipal user, [FromServices] MerchantUseCase usecase) =>
                {
                    string? userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? user.FindFirst("sub")?.Value;

                    if (string.IsNullOrEmpty(userId))
                        return Results.Unauthorized();

                    Result<GetMerchantMeResponse> result = await usecase.GetMerchantMeAsync(userId);
                    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("GetMerchantMe")
            .WithTags("Merchant")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/api/merchant/{merchantId:guid}",
                async ([FromRoute] Guid merchantId, [FromServices] MerchantUseCase usecase) =>
                {
                    Result<GetMerchantByIdResponse> result = await usecase.GetMerchantByIdAsync(merchantId);
                    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("GetMerchantById")
            .WithTags("Merchant")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapPut("/api/merchant/{merchantId:guid}",
                async ([FromRoute] Guid merchantId, [FromBody] UpdateMerchantRequest request,
                    [FromServices] MerchantUseCase usecase) =>
                {
                    request.Id = merchantId;
                    Result<UpdateMerchantResponse> result = await usecase.UpdateMerchantAsync(request);
                    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("UpdateMerchant")
            .WithTags("Merchant")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapDelete("/api/merchant/{merchantId:guid}",
                async ([FromRoute] Guid merchantId, [FromServices] MerchantUseCase usecase) =>
                {
                    Result<DeleteMerchantResponse> result = await usecase.DeleteMerchantAsync(merchantId);
                    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("DeleteMerchant")
            .WithTags("Merchant")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}