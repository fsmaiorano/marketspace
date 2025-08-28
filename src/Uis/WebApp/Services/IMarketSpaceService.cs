using WebApp.Dtos;

namespace WebApp.Services;

public interface IMarketSpaceService
{
    Task<GetCatalogResponse> GetProductsAsync();
}