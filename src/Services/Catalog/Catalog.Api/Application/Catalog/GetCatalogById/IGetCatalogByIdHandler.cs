using BuildingBlocks;
using Merchant.Api.Application.Merchant.GetMerchantById;

namespace Catalog.Api.Application.Catalog.GetCatalogById;

public interface IGetCatalogByIdHandler
{
    Task<Result<GetCatalogByIdResult>> HandleAsync(GetCatalogByIdQuery query);
}