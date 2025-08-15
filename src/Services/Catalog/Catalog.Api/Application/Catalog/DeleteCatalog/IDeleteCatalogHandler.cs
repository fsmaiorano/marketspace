using BuildingBlocks;

namespace Catalog.Api.Application.Catalog.DeleteCatalog;

public interface IDeleteCatalogHandler
{
    Task<Result<DeleteCatalogResult>> HandleAsync(DeleteCatalogCommand command);
}