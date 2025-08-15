using BuildingBlocks;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;

namespace Catalog.Api.Application.Catalog.DeleteCatalog;

public class DeleteCatalogHandler(ICatalogRepository repository, ILogger<DeleteCatalogHandler> logger)
    : IDeleteCatalogHandler
{
    public async Task<Result<DeleteCatalogResult>> HandleAsync(DeleteCatalogCommand command)
    {
        try
        {
            CatalogId catalogId = CatalogId.Of(command.Id);

            await repository.RemoveAsync(catalogId);
            logger.LogInformation("Catalog deleted successfully: {CatalogId}", command.Id);
            return Result<DeleteCatalogResult>.Success(new DeleteCatalogResult(true));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting the catalog: {Command}", command);
            return Result<DeleteCatalogResult>.Failure("An error occurred while deleting the catalog.");
        }
    }
}