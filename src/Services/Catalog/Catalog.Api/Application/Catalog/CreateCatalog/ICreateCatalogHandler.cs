using BuildingBlocks;

namespace Catalog.Api.Application.Catalog.CreateCatalog;

public interface ICreateCatalogHandler
{
    Task<Result<CreateCatalogResult>> HandleAsync(CreateCatalogCommand command);
}