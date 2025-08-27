using BackendForFrontend.Api.Catalog.Dtos;
using BuildingBlocks;

namespace BackendForFrontend.Api.Catalog.Contracts;

public interface ICatalogService
{
    Task<Result<CreateCatalogResponse>> CreateCatalogAsync(CreateCatalogRequest request);
    Task<Result<GetCatalogResponse>> GetCatalogByIdAsync(Guid catalogId);
    Task<Result<GetCatalogListResponse>> GetCatalogListAsync(int pageIndex, int pageSize);
    Task<Result<UpdateCatalogResponse>> UpdateCatalogAsync(UpdateCatalogRequest request);
    Task<Result<DeleteCatalogResponse>> DeleteCatalogAsync(Guid catalogId);
}