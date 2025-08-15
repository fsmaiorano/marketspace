using BuildingBlocks;

namespace Catalog.Api.Application.Catalog.UpdateCatalog;

public interface IUpdateCatalogHandler
{
    Task<Result<UpdateCatalogResult>> HandleAsync(UpdateCatalogCommand command);
}