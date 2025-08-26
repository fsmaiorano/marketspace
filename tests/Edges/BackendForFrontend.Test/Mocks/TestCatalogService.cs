using BackendForFrontend.Api.Catalog.Contracts;
using BackendForFrontend.Api.Catalog.Dtos;
using Microsoft.Extensions.Logging;

namespace BackendForFrontend.Test.Mocks;

public class TestCatalogService(HttpClient httpClient, ILogger<TestCatalogService> logger) : ICatalogService
{
    public Task<CreateCatalogResponse> CreateCatalogAsync(CreateCatalogRequest request)
    {
        logger.LogInformation("Mock: Creating catalog with name: {Name}", request.Name);

        CreateCatalogResponse response = new CreateCatalogResponse
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Category = request.Category,
            Summary = request.Summary,
            ImageFile = request.ImageFile
        };

        return Task.FromResult(response);
    }

    public Task<GetCatalogResponse> GetCatalogByIdAsync(Guid catalogId)
    {
        logger.LogInformation("Mock: Retrieving catalog with ID: {CatalogId}", catalogId);

        GetCatalogResponse response = new GetCatalogResponse
        {
            Id = catalogId,
            Name = "Test Product",
            Description = "Test product description",
            Price = 99.99m,
            Categories = ["Test Category"],
            ImageUrl = "test-image.jpg",
            MerchantId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return Task.FromResult(response);
    }

    public Task<GetCatalogListResponse> GetCatalogListAsync(int pageIndex, int pageSize)
    {
        logger.LogInformation("Mock: Retrieving catalog list with pageIndex: {PageIndex}, pageSize: {PageSize}",
            pageIndex, pageSize);

        GetCatalogListResponse response = new GetCatalogListResponse
        {
            Items =
            [
                new CatalogItemDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Product 1",
                    Description = "Test product 1 description",
                    Price = 99.99m,
                    Categories = ["Test Category"],
                    ImageUrl = "test-image.jpg",
                    MerchantId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                new CatalogItemDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Product 2",
                    Description = "Test product 2 description",
                    Price = 149.99m,
                    Categories = ["Test Category"],
                    ImageUrl = "test-image.jpg",
                    MerchantId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            ]
        };

        return Task.FromResult(response);
    }

    public Task<UpdateCatalogResponse> UpdateCatalogAsync(UpdateCatalogRequest request)
    {
        logger.LogInformation("Mock: Updating catalog with ID: {CatalogId}", request.Id);

        UpdateCatalogResponse response = new UpdateCatalogResponse
        {
            Id = request.Id,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Category = request.Category,
            Summary = request.Summary,
            ImageFile = request.ImageFile
        };

        return Task.FromResult(response);
    }

    public Task<DeleteCatalogResponse> DeleteCatalogAsync(Guid catalogId)
    {
        logger.LogInformation("Mock: Deleting catalog with ID: {CatalogId}", catalogId);

        DeleteCatalogResponse response = new DeleteCatalogResponse { IsSuccess = true };

        return Task.FromResult(response);
    }

    public Task<GetCatalogListResponse> GetCatalogListAsync()
    {
        logger.LogInformation("Mock: Retrieving catalog list");

        GetCatalogListResponse response = new GetCatalogListResponse
        {
            Items =
            [
                new CatalogItemDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Product 1",
                    Description = "Test product 1 description",
                    Price = 99.99m,
                    Categories = ["Test Category"],
                    ImageUrl = "test-image.jpg",
                    MerchantId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                new CatalogItemDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Product 2",
                    Description = "Test product 2 description",
                    Price = 149.99m,
                    Categories = ["Test Category"],
                    ImageUrl = "test-image.jpg",
                    MerchantId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            ]
        };

        return Task.FromResult(response);
    }
}