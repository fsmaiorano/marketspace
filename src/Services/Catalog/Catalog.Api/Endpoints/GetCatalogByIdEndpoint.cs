using BuildingBlocks;
using Catalog.Api.Application.Catalog.GetCatalogById;
using Catalog.Api.Endpoints.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Catalog.Api.Endpoints;

public static class GetCatalogByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/catalog/{id:guid}", async ([FromRoute] Guid id, [FromServices] GetCatalogById handler) =>
            {
                GetCatalogByIdQuery query = new(id);
                Result<GetCatalogByIdResult> result = await handler.HandleAsync(query);
                return result is { IsSuccess: true, Data: not null }
                    ? Results.Ok(new CatalogDto()
                    {
                        Id = result.Data.Id,
                        Name = result.Data.Name,
                        Price = result.Data.Price,
                        ImageUrl = result.Data.ImageUrl,
                        MerchantId = result.Data.MerchantId,
                        Description = result.Data.Description,
                        Categories = result.Data!.Categories.ToList(),
                        CreatedAt = result.Data.CreatedAt,
                        UpdatedAt = result.Data.UpdatedAt
                    })
                    : Results.NotFound(result.Error);
            })
            .WithName("GetCatalogById")
            .WithTags("Catalog")
            .Produces<GetCatalogByIdResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}