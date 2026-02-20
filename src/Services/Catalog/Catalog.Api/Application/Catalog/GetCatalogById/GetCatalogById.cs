using BuildingBlocks;
using BuildingBlocks.Loggers;
using BuildingBlocks.Storage.Minio;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Catalog.Api.Application.Catalog.GetCatalogById;

public record GetCatalogByIdQuery(Guid Id);

public record GetCatalogByIdResult(
    Guid Id,
    string Name,
    string Description,
    string ImageUrl,
    decimal Price,
    [property: JsonPropertyName("categories")]
    IReadOnlyList<string> Categories,
    Guid MerchantId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public class GetCatalogById(
    ICatalogRepository repository,
    IAppLogger<GetCatalogById> logger,
    IMinioBucket minioBucket)
{
    public async Task<Result<GetCatalogByIdResult>> HandleAsync(GetCatalogByIdQuery query)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing get catalog by ID request for: {CatalogId}",
                query.Id);

            CatalogEntity? catalog = await repository.GetByIdAsync(CatalogId.Of(query.Id), isTrackingEnabled: false);

            if (catalog is null)
                return Result<GetCatalogByIdResult>.Failure($"Catalog with ID {query.Id} not found.");

            string? getImage = await minioBucket.GetImageAsync(catalog.ImageUrl);
            string getImageToDownload = await minioBucket.GetImageToDownload(catalog.ImageUrl);
            
            Console.WriteLine(getImage);
            Console.WriteLine(getImageToDownload);

            GetCatalogByIdResult result = new(
                Id: catalog.Id.Value,
                Name: catalog.Name,
                Description: catalog.Description,
                ImageUrl: catalog.ImageUrl,
                Price: catalog.Price.Value,
                Categories: new ReadOnlyCollection<string>(catalog.Categories),
                MerchantId: catalog.MerchantId,
                CreatedAt: catalog.CreatedAt,
                UpdatedAt: catalog.UpdatedAt
            );

            return Result<GetCatalogByIdResult>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while getting catalog by ID.");
            return Result<GetCatalogByIdResult>.Failure("An error occurred while processing your request.");
        }
    }
}