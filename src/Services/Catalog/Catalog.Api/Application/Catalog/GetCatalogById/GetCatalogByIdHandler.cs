using BuildingBlocks;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;
using Merchant.Api.Application.Merchant.GetMerchantById;
using System.Collections.ObjectModel;

namespace Catalog.Api.Application.Catalog.GetCatalogById;

public class GetCatalogByIdHandler(ICatalogRepository repository, ILogger<GetCatalogByIdHandler> logger)
    : IGetCatalogByIdHandler
{
    public async Task<Result<GetCatalogByIdResult>> HandleAsync(GetCatalogByIdQuery query)
    {
        try
        {
            CatalogId catalogId = CatalogId.Of(query.Id);

            CatalogEntity? catalog = await repository.GetByIdAsync(catalogId, isTrackingEnabled: false);

            if (catalog is null)
                return Result<GetCatalogByIdResult>.Failure($"Catalog with ID {query.Id} not found.");

            GetCatalogByIdResult result = new GetCatalogByIdResult
            {
                Id = catalog.Id.Value,
                Name = catalog.Name,
                Description = catalog.Description,
                ImageUrl = catalog.ImageUrl,
                Price = catalog.Price.Value,
                Categories = new ReadOnlyCollection<string>(catalog.Categories.ToList())
            };

            return Result<GetCatalogByIdResult>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while getting catalog by ID.");
            return Result<GetCatalogByIdResult>.Failure("An error occurred while processing your request.");
        }
    }
}