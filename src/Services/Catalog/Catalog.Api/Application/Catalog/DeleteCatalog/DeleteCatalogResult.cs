namespace Catalog.Api.Application.Catalog.DeleteCatalog;

public class DeleteCatalogResult(bool isSuccess)
{
    public bool IsSuccess { get; init; } = isSuccess;
}