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

public class GetCatalogByIdResult
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public decimal Price { get; init; } = 0.0m;
    
    [JsonPropertyName("categories")]
    public IReadOnlyList<string> Categories { get; init; } = [];
    public Guid MerchantId { get; init; } = Guid.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public class GetCatalogById(
    ICatalogRepository repository,
    IAppLogger<GetCatalogById> logger,
    IMinioBucket minioBucket)
{
    public async Task<Result<GetCatalogByIdResult>> HandleAsync(GetCatalogByIdQuery query)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing get catalog by ID request for: {CatalogId}", query.Id);

            CatalogId catalogId = CatalogId.Of(query.Id);

            CatalogEntity? catalog = await repository.GetByIdAsync(catalogId, isTrackingEnabled: false);

            if (catalog is null)
                return Result<GetCatalogByIdResult>.Failure($"Catalog with ID {query.Id} not found.");

            var x = await minioBucket.GetImageAsync(catalog.ImageUrl);
            var y = await minioBucket.GetImageToDownload(catalog.ImageUrl);

            GetCatalogByIdResult result = new()

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
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while getting catalog by ID.");
            return Result<GetCatalogByIdResult>.Failure("An error occurred while processing your request.");
        }
    }
}