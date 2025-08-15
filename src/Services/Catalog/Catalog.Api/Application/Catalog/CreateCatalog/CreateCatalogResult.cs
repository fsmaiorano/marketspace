namespace Catalog.Api.Application.Catalog.CreateCatalog;

public class CreateCatalogResult(Guid merchantId)
{
    public Guid MerchantId { get; init; } = merchantId;
}