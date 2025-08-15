using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.ValueObjects;

namespace Catalog.Api.Domain.Repositories;

public interface ICatalogRepository
{
    Task<int> AddAsync(CatalogEntity merchant, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(CatalogEntity merchant, CancellationToken cancellationToken = default);
    Task<int> RemoveAsync(CatalogId id, CancellationToken cancellationToken = default);
    Task<CatalogEntity?> GetByIdAsync(CatalogId id,bool isTrackingEnabled , CancellationToken cancellationToken = default);
}