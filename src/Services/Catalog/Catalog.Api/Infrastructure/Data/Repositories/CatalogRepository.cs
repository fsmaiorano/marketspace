using BuildingBlocks.Pagination;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Infrastructure.Data.Repositories;

public class CatalogRepository(ICatalogDbContext dbContext) : ICatalogRepository
{
    public async Task<int> AddAsync(CatalogEntity catalog, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(catalog, nameof(catalog));
        catalog.Id = CatalogId.Of(Guid.CreateVersion7());
        await dbContext.Catalogs.AddAsync(catalog, cancellationToken);
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> UpdateAsync(CatalogEntity catalog, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(catalog, nameof(catalog));

        CatalogEntity storedEntity = await GetByIdAsync(catalog.Id, cancellationToken: cancellationToken)
                                     ?? throw new InvalidOperationException(
                                         $"Catalog with ID {catalog.Id} not found.");

        storedEntity.Update(
            catalog.Name,
            catalog.Categories,
            catalog.Description,
            catalog.ImageUrl,
            catalog.Price);

        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> RemoveAsync(CatalogId id, CancellationToken cancellationToken = default)
    {
        CatalogEntity? storedEntity = await GetByIdAsync(id, cancellationToken: cancellationToken)
                                      ?? throw new InvalidOperationException(
                                          $"Catalog with ID {id} not found.");

        dbContext.Catalogs.Remove(storedEntity);
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CatalogEntity?> GetByIdAsync(CatalogId id, bool isTrackingEnabled = true,
        CancellationToken cancellationToken = default)
    {
        if (isTrackingEnabled)
            return await dbContext.Catalogs.FirstOrDefaultAsync(m => m.Id.Equals(id),
                cancellationToken: cancellationToken);

        return await dbContext.Catalogs.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id.Equals(id), cancellationToken);
    }

    public async Task<PaginatedResult<CatalogEntity>> GetPaginatedListAsync(PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        IQueryable<CatalogEntity> query = dbContext.Catalogs.AsNoTracking();

        int totalItems = await query.CountAsync(cancellationToken);
        List<CatalogEntity> items = await query
            .AsNoTracking()
            .Skip((pagination.PageIndex - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<CatalogEntity>(count: totalItems, pageIndex: pagination.PageIndex,
            pageSize: pagination.PageSize, data: items);
    }
}