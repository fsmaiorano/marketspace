using BuildingBlocks;
using BuildingBlocks.Loggers.Abstractions;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;

namespace Catalog.Api.Application.Catalog.DeleteCatalog;

public class DeleteCatalogHandler(
    ICatalogRepository repository, 
    IApplicationLogger<DeleteCatalogHandler> applicationLogger,
    IBusinessLogger<DeleteCatalogHandler> businessLogger)
    : IDeleteCatalogHandler
{
    public async Task<Result<DeleteCatalogResult>> HandleAsync(DeleteCatalogCommand command)
    {
        try
        {
            applicationLogger.LogInformation("Processing delete catalog request for: {CatalogId}", command.Id);
            
            CatalogId catalogId = CatalogId.Of(command.Id);

            await repository.RemoveAsync(catalogId);
            
            businessLogger.LogInformation("Catalog deleted successfully. CatalogId: {CatalogId}", command.Id);
            return Result<DeleteCatalogResult>.Success(new DeleteCatalogResult(true));
        }
        catch (Exception ex)
        {
            applicationLogger.LogError(ex, "An error occurred while deleting the catalog: {Command}", command);
            return Result<DeleteCatalogResult>.Failure("An error occurred while deleting the catalog.");
        }
    }
}