using BuildingBlocks;
using BuildingBlocks.Loggers.Abstractions;
using BuildingBlocks.Storage.Minio;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;
using System.Collections.ObjectModel;

namespace Catalog.Api.Application.Catalog.GetCatalogById;

public class GetCatalogByIdHandler(
    ICatalogRepository repository,
    IApplicationLogger<GetCatalogByIdHandler> applicationLogger,
    IBusinessLogger<GetCatalogByIdHandler> businessLogger,
    IMinioBucket minioBucket)
    : IGetCatalogByIdHandler
{
    public async Task<Result<GetCatalogByIdResult>> HandleAsync(GetCatalogByIdQuery query)
    {
        try
        {
            applicationLogger.LogInformation("Processing get catalog by ID request for: {CatalogId}", query.Id);
            
            CatalogId catalogId = CatalogId.Of(query.Id);

            CatalogEntity? catalog = await repository.GetByIdAsync(catalogId, isTrackingEnabled: false);

            if (catalog is null)
                return Result<GetCatalogByIdResult>.Failure($"Catalog with ID {query.Id} not found.");
            
            var x = await minioBucket.GetImageAsync(catalog.ImageUrl);
            var y = await minioBucket.GetImageToDownload(catalog.ImageUrl);

            GetCatalogByIdResult result = new GetCatalogByIdResult
            {
                Id = catalog.Id.Value,
                Name = catalog.Name,
                Description = catalog.Description,
                ImageUrl = catalog.ImageUrl,
                Price = catalog.Price.Value,
                Categories = new ReadOnlyCollection<string>(catalog.Categories.ToList())
            };

            return Result<GetCatalogByIdResult>.Success(result);
        }
        catch (Exception ex)
        {
            applicationLogger.LogError(ex, "An error occurred while getting catalog by ID.");
            return Result<GetCatalogByIdResult>.Failure("An error occurred while processing your request.");
        }
    }
}