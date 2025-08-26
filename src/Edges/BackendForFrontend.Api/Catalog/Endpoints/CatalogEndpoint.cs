using BackendForFrontend.Api.Catalog.Contracts;
using BackendForFrontend.Api.Catalog.Dtos;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks;
using BuildingBlocks.Pagination;

namespace BackendForFrontend.Api.Catalog.Endpoints;

public static class CatalogEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/catalog",
                async ([FromBody] CreateCatalogRequest request, [FromServices] ICatalogUseCase usecase) =>
                {
                    CreateCatalogResponse result = await usecase.CreateCatalogAsync(request);
                    return Results.Ok(Result<CreateCatalogResponse>.Success(result));
                })
            .WithName("CreateCatalog")
            .WithTags("Catalog")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/api/catalog/{catalogId:guid}",
                async ([FromRoute] Guid catalogId, [FromServices] ICatalogUseCase usecase) =>
                {
                    GetCatalogResponse result = await usecase.GetCatalogByIdAsync(catalogId);
                    return Results.Ok(Result<GetCatalogResponse>.Success(result));
                })
            .WithName("GetCatalogById")
            .WithTags("Catalog")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/api/catalog",
                async ([AsParameters] PaginationRequest pagination, [FromServices] ICatalogUseCase usecase) =>
                {
                    GetCatalogListResponse result =
                        await usecase.GetCatalogListAsync(pagination.PageIndex, pagination.PageSize);
                    return Results.Ok(Result<GetCatalogListResponse>.Success(result));
                })
            .WithName("GetCatalog")
            .WithTags("Catalog")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapPut("/api/catalog/{catalogId:guid}",
                async ([FromRoute] Guid catalogId, [FromBody] UpdateCatalogRequest request,
                    [FromServices] ICatalogUseCase usecase) =>
                {
                    request.Id = catalogId;
                    UpdateCatalogResponse result = await usecase.UpdateCatalogAsync(request);
                    return Results.Ok(Result<UpdateCatalogResponse>.Success(result));
                })
            .WithName("UpdateCatalog")
            .WithTags("Catalog")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapDelete("/api/catalog/{catalogId:guid}",
                async ([FromRoute] Guid catalogId, [FromServices] ICatalogUseCase usecase) =>
                {
                    DeleteCatalogResponse result = await usecase.DeleteCatalogAsync(catalogId);
                    return Results.Ok(Result<DeleteCatalogResponse>.Success(result));
                })
            .WithName("DeleteCatalog")
            .WithTags("Catalog")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}