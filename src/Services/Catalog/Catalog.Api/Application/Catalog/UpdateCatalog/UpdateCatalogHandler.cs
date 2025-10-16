using BuildingBlocks;
using BuildingBlocks.Loggers.Abstractions;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;

namespace Catalog.Api.Application.Catalog.UpdateCatalog;

public sealed class UpdateCatalogHandler(
    ICatalogRepository repository, 
    IApplicationLogger<UpdateCatalogHandler> applicationLogger,
    IBusinessLogger<UpdateCatalogHandler> businessLogger)
    : IUpdateCatalogHandler
{
    public async Task<Result<UpdateCatalogResult>> HandleAsync(UpdateCatalogCommand command)
    {
        try
        {
            applicationLogger.LogInformation("Processing update catalog request for: {CatalogId}", command.Id);
            
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
            
            businessLogger.LogInformation("Catalog updated successfully. CatalogId: {CatalogId}, Name: {Name}", 
                command.Id, 
                command.Name);

            return Result<UpdateCatalogResult>.Success(new UpdateCatalogResult(true));
        }
        catch (Exception ex)
        {
            applicationLogger.LogError(ex, "An error occurred while updating the catalog: {Command}", command);
            return Result<UpdateCatalogResult>.Failure("An error occurred while updating the catalog.");
        }
    }
}