namespace Catalog.Api.Application.Catalog.UpdateCatalog;

public class UpdateCatalogResult(bool isSuccess)
{
    public bool IsSuccess { get; init; } = isSuccess;
}