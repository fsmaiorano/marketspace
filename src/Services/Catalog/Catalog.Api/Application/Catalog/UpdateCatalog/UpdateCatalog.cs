using BuildingBlocks;
using BuildingBlocks.Loggers;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;

namespace Catalog.Api.Application.Catalog.UpdateCatalog;

public record UpdateCatalogCommand
{
    public required Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public decimal Price { get; init; } = 0.0m;
    public List<string> Categories { get; init; } = [];
    public Guid MerchantId { get; init; } = Guid.Empty;
}

public record UpdateCatalogResult();

public sealed class UpdateCatalog(
    ICatalogRepository repository,
    IAppLogger<UpdateCatalog> logger)
{
    public async Task<Result<UpdateCatalogResult>> HandleAsync(UpdateCatalogCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing update catalog request for: {CatalogId}",
                command.Id);

            CatalogId catalogId = CatalogId.Of(command.Id);
            CatalogEntity? catalogEntity =
                await repository.GetByIdAsync(catalogId, isTrackingEnabled: true, CancellationToken.None);

            if (catalogEntity == null)
            {
                logger.LogWarning(LogTypeEnum.Application, "Catalog not found for update: {CatalogId}", command.Id);
                return Result<UpdateCatalogResult>.Failure($"Catalog with ID {command.Id} not found.");
            }

            catalogEntity.Update(
                name: command.Name,
                description: command.Description,
                imageUrl: command.ImageUrl,
                price: Price.Of(command.Price),
                categories: command.Categories,
                merchantId: command.MerchantId
            );

            await repository.UpdateAsync(catalogEntity);

            logger.LogInformation(LogTypeEnum.Business,
                "Catalog updated successfully. CatalogId: {CatalogId}, Name: {Name}",
                command.Id,
                command.Name);

            return Result<UpdateCatalogResult>.Success(new UpdateCatalogResult());
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while updating the catalog: {Command}",
                command);
            return Result<UpdateCatalogResult>.Failure("An error occurred while updating the catalog.");
        }
    }
}