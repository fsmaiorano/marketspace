using WebApp.Dtos;

namespace WebApp.Services;

public interface IMarketSpaceService
{
    Task<GetCatalogResponse> GetProductsAsync();
    Task<GetCatalogResponse> GetProductsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}