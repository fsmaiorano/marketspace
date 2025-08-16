using BuildingBlocks;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;

namespace Catalog.Api.Application.Catalog.UpdateCatalog;

public sealed class UpdateCatalogHandler(ICatalogRepository repository, ILogger<UpdateCatalogHandler> logger)
    : IUpdateCatalogHandler
{
    public async Task<Result<UpdateCatalogResult>> HandleAsync(UpdateCatalogCommand command)
    {
        try
        {
            CatalogEntity catalogEntity = CatalogEntity.Create(
                name: command.Name,
                description: command.Description,
                imageUrl: command.ImageUrl,
                categories: command.Categories,
                price: Price.Of(command.Price),
                merchantId: command.MerchantId
            );
            
            catalogEntity.Id = CatalogId.Of(command.Id);

            await repository.UpdateAsync(catalogEntity);
            logger.LogInformation("Catalog updated successfully: {CatalogId}", command.Id);

            return Result<UpdateCatalogResult>.Success(new UpdateCatalogResult(true));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating the catalog: {Command}", command);
            return Result<UpdateCatalogResult>.Failure("An error occurred while updating the catalog.");
        }
    }
}