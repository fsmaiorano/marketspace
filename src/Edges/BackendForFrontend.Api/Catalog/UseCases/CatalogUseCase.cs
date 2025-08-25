using BackendForFrontend.Api.Catalog.Contracts;
using BackendForFrontend.Api.Catalog.Dtos;

namespace BackendForFrontend.Api.Catalog.UseCases;

public class CatalogUseCase(ILogger<CatalogUseCase> logger, ICatalogService service) : ICatalogUseCase
{
    public async Task<CreateCatalogResponse> CreateCatalogAsync(CreateCatalogRequest request)
    {
        logger.LogInformation("Creating catalog with name: {Name}", request.Name);
        return await service.CreateCatalogAsync(request);
    }

    public async Task<GetCatalogResponse> GetCatalogByIdAsync(Guid catalogId)
    {
        logger.LogInformation("Retrieving catalog with ID: {CatalogId}", catalogId);
        return await service.GetCatalogByIdAsync(catalogId);
    }

    public async Task<UpdateCatalogResponse> UpdateCatalogAsync(UpdateCatalogRequest request)
    {
        logger.LogInformation("Updating catalog with ID: {CatalogId}", request.Id);
        return await service.UpdateCatalogAsync(request);
    }

    public async Task<DeleteCatalogResponse> DeleteCatalogAsync(Guid catalogId)
    {
        logger.LogInformation("Deleting catalog with ID: {CatalogId}", catalogId);
        return await service.DeleteCatalogAsync(catalogId);
    }

    public async Task<GetCatalogListResponse> GetCatalogListAsync()
    {
        logger.LogInformation("Retrieving catalog list");
        return await service.GetCatalogListAsync();
    }
}
