namespace Catalog.Api.Application.Catalog.CreateCatalog;

public class CreateCatalogResult(Guid catalogId)
{
    public Guid CatalogId { get; init; } = catalogId;
}