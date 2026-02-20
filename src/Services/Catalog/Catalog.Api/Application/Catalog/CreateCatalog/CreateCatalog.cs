using BuildingBlocks;
using BuildingBlocks.Loggers;
using BuildingBlocks.Storage.Minio;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;

namespace Catalog.Api.Application.Catalog.CreateCatalog;

public record CreateCatalogCommand
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string ImageUrl { get; init; }
    public required Price Price { get; init; }

    public IReadOnlyList<string> Categories { get; init; } = [];

    public required Guid MerchantId { get; init; }
}

public record CreateCatalogResult();

public sealed class CreateCatalog(
    ICatalogRepository repository,
    IAppLogger<CreateCatalog> logger,
    IMinioBucket minioBucket
)
{
    public async Task<Result<CreateCatalogResult>> HandleAsync(CreateCatalogCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing create catalog request for: {Name}",
                command.Name);

            (string objectName, string _) = await minioBucket.SendImageAsync(command.ImageUrl);

            if (string.IsNullOrEmpty(objectName))
            {
                logger.LogError(LogTypeEnum.Application, null, "Image upload failed for command: {Command}", command);
                return Result<CreateCatalogResult>.Failure("Image upload failed.");
            }

            CatalogEntity catalogEntity = CatalogEntity.Create(
                name: command.Name,
                description: command.Description,
                imageUrl: objectName,
                categories: command.Categories,
                price: Price.Of(command.Price.Value),
                merchantId: command.MerchantId
            );

            int result = await repository.AddAsync(catalogEntity);

            if (result <= 0)
            {
                logger.LogError(LogTypeEnum.Application, null, "Failed to persist catalog to database: {Command}",
                    command);
                return Result<CreateCatalogResult>.Failure("Failed to create catalog.");
            }

            logger.LogInformation(LogTypeEnum.Business,
                "Catalog created successfully. CatalogId: {CatalogId}, Name: {Name}, Price: {Price}, MerchantId: {MerchantId}",
                catalogEntity.Id,
                command.Name,
                command.Price,
                command.MerchantId);

            return Result<CreateCatalogResult>.Success(new CreateCatalogResult());
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while creating the catalog: {Command}",
                command);
            return Result<CreateCatalogResult>.Failure($"An error occurred while creating the catalog: {ex.Message}");
        }
    }
}