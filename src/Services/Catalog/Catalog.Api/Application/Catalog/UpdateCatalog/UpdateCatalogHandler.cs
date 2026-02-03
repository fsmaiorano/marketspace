using BuildingBlocks;
using BuildingBlocks.Loggers;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;

namespace Catalog.Api.Application.Catalog.UpdateCatalog;

public sealed class UpdateCatalogHandler(
    ICatalogRepository repository, 
    IAppLogger<UpdateCatalogHandler> logger)
    : IUpdateCatalogHandler
{
    public async Task<Result<UpdateCatalogResult>> HandleAsync(UpdateCatalogCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing update catalog request for: {CatalogId}", command.Id);
            
            // Buscar entidade existente rastreada
            CatalogId catalogId = CatalogId.Of(command.Id);
            CatalogEntity? catalogEntity = await repository.GetByIdAsync(catalogId, isTrackingEnabled: true, CancellationToken.None);
            
            if (catalogEntity == null)
            {
                logger.LogWarning(LogTypeEnum.Application, "Catalog not found for update: {CatalogId}", command.Id);
                return Result<UpdateCatalogResult>.Failure($"Catalog with ID {command.Id} not found.");
            }
            
            // Usar método de domínio para atualizar
            catalogEntity.Update(
                command.Name,
                command.Categories,
                command.Description,
                command.ImageUrl,
                Price.Of(command.Price));

            await repository.UpdateAsync(catalogEntity);
            
            logger.LogInformation(LogTypeEnum.Business, "Catalog updated successfully. CatalogId: {CatalogId}, Name: {Name}", 
                command.Id, 
                command.Name);

            return Result<UpdateCatalogResult>.Success(new UpdateCatalogResult(true));
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while updating the catalog: {Command}", command);
            return Result<UpdateCatalogResult>.Failure("An error occurred while updating the catalog.");
        }
    }
}