using BuildingBlocks;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;

namespace Catalog.Api.Application.Catalog.CreateCatalog;

public sealed class CreateCatalogHandler(ICatalogRepository repository, ILogger<CreateCatalogHandler> logger)
    : ICreateCatalogHandler
{
    public async Task<Result<CreateCatalogResult>> HandleAsync(CreateCatalogCommand command)
    {
        try
        {
            CatalogEntity catalogEntity = CatalogEntity.Create(
                name: command.Name,
                description: command.Description,
                imageUrl: command.ImageUrl,
                categories: command.Categories,
                price: Price.Of(command.Price)
            );

            int result = await repository.AddAsync(catalogEntity);

            if (result <= 0)
            {
                logger.LogError("Failed to create catalog: {Command}", command);
                return Result<CreateCatalogResult>.Failure("Failed to create catalog.");
            }

            logger.LogInformation("Catalog created successfully: {CatalogId}", catalogEntity.Id);
            return Result<CreateCatalogResult>.Success(new CreateCatalogResult(catalogEntity.Id.Value));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the catalog: {Command}", command);
            return Result<CreateCatalogResult>.Failure($"An error occurred while creating the catalog: {ex.Message}");
        }
    }
}