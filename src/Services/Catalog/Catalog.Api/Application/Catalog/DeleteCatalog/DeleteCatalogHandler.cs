using BuildingBlocks;
using BuildingBlocks.Loggers;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;

namespace Catalog.Api.Application.Catalog.DeleteCatalog;

public class DeleteCatalogHandler(
    ICatalogRepository repository, 
    IAppLogger<DeleteCatalogHandler> logger)
    : IDeleteCatalogHandler
{
    public async Task<Result<DeleteCatalogResult>> HandleAsync(DeleteCatalogCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing delete catalog request for: {CatalogId}", command.Id);
            
            CatalogId catalogId = CatalogId.Of(command.Id);

            await repository.RemoveAsync(catalogId);
            
            logger.LogInformation(LogTypeEnum.Business, "Catalog deleted successfully. CatalogId: {CatalogId}", command.Id);
            return Result<DeleteCatalogResult>.Success(new DeleteCatalogResult(true));
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while deleting the catalog: {Command}", command);
            return Result<DeleteCatalogResult>.Failure("An error occurred while deleting the catalog.");
        }
    }
}