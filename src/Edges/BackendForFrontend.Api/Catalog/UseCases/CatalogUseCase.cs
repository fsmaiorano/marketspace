using BackendForFrontend.Api.Catalog.Contracts;
using BackendForFrontend.Api.Catalog.Dtos;
using BuildingBlocks;
using BuildingBlocks.Loggers.Abstractions;

namespace BackendForFrontend.Api.Catalog.UseCases;

public class CatalogUseCase(
    IApplicationLogger<CatalogUseCase> applicationLogger,
    IBusinessLogger<CatalogUseCase> businessLogger,
    ICatalogService service) : ICatalogUseCase
{
    public async Task<Result<CreateCatalogResponse>> CreateCatalogAsync(CreateCatalogRequest request)
    {
        applicationLogger.LogInformation("Creating catalog with name: {Name}", request.Name);
        return await service.CreateCatalogAsync(request);
    }

    public async Task<Result<GetCatalogResponse>> GetCatalogByIdAsync(Guid catalogId)
    {
        applicationLogger.LogInformation("Retrieving catalog with ID: {CatalogId}", catalogId);
        return await service.GetCatalogByIdAsync(catalogId);
    }
    
    public async Task<Result<GetCatalogListResponse>> GetCatalogListAsync(int pageIndex, int pageSize) 
    {
        applicationLogger.LogInformation("Retrieving catalog list with pageIndex: {PageIndex}, pageSize: {PageSize}", pageIndex, pageSize);
        return await service.GetCatalogListAsync(pageIndex, pageSize);
    }

    public async Task<Result<UpdateCatalogResponse>> UpdateCatalogAsync(UpdateCatalogRequest request)
    {
        applicationLogger.LogInformation("Updating catalog with ID: {CatalogId}", request.Id);
        return await service.UpdateCatalogAsync(request);
    }

    public async Task<Result<DeleteCatalogResponse>> DeleteCatalogAsync(Guid catalogId)
    {
        applicationLogger.LogInformation("Deleting catalog with ID: {CatalogId}", catalogId);
        return await service.DeleteCatalogAsync(catalogId);
    }
}
