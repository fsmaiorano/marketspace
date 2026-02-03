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
        ArgumentNullException.ThrowIfNull(catalog);
        catalog.Id = CatalogId.Of(Guid.CreateVersion7());
        await dbContext.Catalogs.AddAsync(catalog, cancellationToken);
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> UpdateAsync(CatalogEntity catalog, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> RemoveAsync(CatalogId id, CancellationToken cancellationToken = default)
    {
        CatalogEntity storedEntity = await GetByIdAsync(id, isTrackingEnabled: true, cancellationToken)
            ?? throw new InvalidOperationException($"Catalog with ID {id} not found.");

        dbContext.Catalogs.Remove(storedEntity);
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CatalogEntity?> GetByIdAsync(CatalogId id, bool isTrackingEnabled = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<CatalogEntity> query = dbContext.Catalogs;

        if (!isTrackingEnabled)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(m => m.Id.Equals(id), cancellationToken);
    }

    public async Task<PaginatedResult<CatalogEntity>> GetPaginatedListAsync(PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        IQueryable<CatalogEntity> query = dbContext.Catalogs.AsNoTracking();

        int totalItems = await query.CountAsync(cancellationToken);
        List<CatalogEntity> items = await query
            .Skip((pagination.PageIndex - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<CatalogEntity>(count: totalItems, pageIndex: pagination.PageIndex,
            pageSize: pagination.PageSize, data: items);
    }
}