using BackendForFrontend.Api.Catalog.Dtos;

namespace BackendForFrontend.Api.Catalog.Contracts;

public interface ICatalogService
{
    Task<CreateCatalogResponse> CreateCatalogAsync(CreateCatalogRequest request);
    Task<GetCatalogResponse> GetCatalogByIdAsync(Guid catalogId);
    Task<GetCatalogListResponse> GetCatalogListAsync(int pageIndex, int pageSize);
    Task<UpdateCatalogResponse> UpdateCatalogAsync(UpdateCatalogRequest request);
    Task<DeleteCatalogResponse> DeleteCatalogAsync(Guid catalogId);
    Task<GetCatalogListResponse> GetCatalogListAsync();
}