using BuildingBlocks;
using BuildingBlocks.Loggers;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;

namespace Catalog.Api.Application.Catalog.DeleteCatalog;

public record DeleteCatalogCommand(Guid Id);

public record DeleteCatalogResult();

public class DeleteCatalog(
    ICatalogRepository repository,
    IAppLogger<DeleteCatalog> logger)
{
    public async Task<Result<DeleteCatalogResult>> HandleAsync(DeleteCatalogCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing delete catalog request for: {CatalogId}",
                command.Id);

            CatalogId catalogId = CatalogId.Of(command.Id);

            await repository.RemoveAsync(catalogId);

            logger.LogInformation(LogTypeEnum.Business, "Catalog deleted successfully. CatalogId: {CatalogId}",
                command.Id);
            return Result<DeleteCatalogResult>.Success(new DeleteCatalogResult());
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while deleting the catalog: {Command}",
                command);
            return Result<DeleteCatalogResult>.Failure("An error occurred while deleting the catalog.");
        }
    }
}