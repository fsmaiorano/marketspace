using BuildingBlocks;

namespace Catalog.Api.Application.Catalog.GetCatalogById;

public interface IGetCatalogByIdHandler
{
    Task<Result<GetCatalogByIdResult>> HandleAsync(GetCatalogByIdQuery query);
}