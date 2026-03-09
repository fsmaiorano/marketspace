using BackendForFrontend.Api.Catalog.Dtos;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks;
using BuildingBlocks.Pagination;
using BackendForFrontend.Api.Catalog.UseCases;
using BackendForFrontend.Api.Services;
using System.Security.Claims;

namespace BackendForFrontend.Api.Catalog.Endpoints;

public static class CatalogEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/catalog",
                async ([FromBody] CreateCatalogRequest request, [FromServices] CatalogUseCase usecase) =>
                {
                    Result<CreateCatalogResponse> result = await usecase.CreateCatalogAsync(request);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("CreateCatalog")
            .WithTags("Catalog")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/api/catalog/{catalogId:guid}",
                async ([FromRoute] Guid catalogId, [FromServices] CatalogUseCase usecase) =>
                {
                    Result<GetCatalogResponse> result = await usecase.GetCatalogByIdAsync(catalogId);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("GetCatalogById")
            .WithTags("Catalog")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/api/catalog",
                async ([AsParameters] PaginationRequest pagination, [FromServices] CatalogUseCase usecase) =>
                {
                    Result<GetCatalogListResponse> result =
                        await usecase.GetCatalogListAsync(pagination.PageIndex, pagination.PageSize);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("GetCatalog")
            .WithTags("Catalog")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapPut("/api/catalog/{catalogId:guid}",
                async ([FromRoute] Guid catalogId, [FromBody] UpdateCatalogRequest request,
                    [FromServices] CatalogUseCase usecase) =>
                {
                    request.Id = catalogId;
                    Result<UpdateCatalogResponse> result = await usecase.UpdateCatalogAsync(request);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("UpdateCatalog")
            .WithTags("Catalog")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapDelete("/api/catalog/{catalogId:guid}",
                async ([FromRoute] Guid catalogId, [FromServices] CatalogUseCase usecase) =>
                {
                    Result<DeleteCatalogResponse> result = await usecase.DeleteCatalogAsync(catalogId);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("DeleteCatalog")
            .WithTags("Catalog")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/api/catalog/merchant/{merchantId:guid}",
                async ([FromRoute] Guid merchantId, [AsParameters] PaginationRequest pagination,
                    [FromServices] CatalogUseCase usecase) =>
                {
                    Result<GetCatalogListResponse> result =
                        await usecase.GetCatalogByMerchantIdAsync(merchantId, pagination.PageIndex, pagination.PageSize);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("GetCatalogByMerchantId")
            .WithTags("Catalog")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapPatch("/api/catalog/{catalogId:guid}/stock",
                async ([FromRoute] Guid catalogId, [FromBody] UpdateStockRequest request,
                    [FromServices] CatalogUseCase usecase,
                    [FromServices] IStockEventService stockEventService,
                    ClaimsPrincipal user) =>
                {
                    request.CatalogId = catalogId;
                    Result<UpdateStockResponse> result = await usecase.UpdateStockAsync(catalogId, request.Delta);
                    if (result.IsSuccess)
                    {
                        string? userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? user.FindFirst("sub")?.Value;
                        if (userId != null && result.Data != null)
                        {
                            await stockEventService.PublishAsync(userId,
                                new StockChangedEvent(catalogId, string.Empty, result.Data.NewStock, DateTimeOffset.UtcNow));
                        }
                        return Results.Ok(result);
                    }
                    return Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("UpdateCatalogStock")
            .WithTags("Catalog")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }
}