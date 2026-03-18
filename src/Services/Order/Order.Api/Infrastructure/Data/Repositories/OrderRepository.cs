using Microsoft.EntityFrameworkCore;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Infrastructure.Data.Repositories;

public class OrderRepository(IOrderDbContext dbContext) : IOrderRepository
{
    public async Task<int> AddAsync(OrderEntity order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);
        await dbContext.Orders.AddAsync(order, cancellationToken);

        int result = await dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<int> UpdateAsync(OrderEntity order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);

        int result = await dbContext.SaveChangesAsync(cancellationToken);


        return result;
    }

    public async Task<int> RemoveAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        OrderEntity storedEntity = await GetByIdAsync(id, isTrackingEnabled: true, cancellationToken)
            ?? throw new InvalidOperationException($"Order with ID {id} not found.");

        dbContext.Orders.Remove(storedEntity);
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrderEntity?> GetByIdAsync(OrderId id, bool isTrackingEnabled = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<OrderEntity> query = dbContext.Orders.Include(o => o.Items);

        if (!isTrackingEnabled)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(m => m.Id.Equals(id), cancellationToken);
    }

    public async Task<List<OrderEntity>> GetByCustomerIdAsync(Guid customerId, int limit = 10,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .Where(o => o.CustomerId.Value == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OrderEntity>> GetRecentByCatalogIdsAsync(IEnumerable<Guid> catalogIds, int limit = 50,
        CancellationToken cancellationToken = default)
    {
        HashSet<Guid> catalogIdSet = catalogIds
            .Where(id => id != Guid.Empty)
            .ToHashSet();

        if (catalogIdSet.Count == 0)
            return [];

        List<OrderEntity> candidateOrders = await dbContext.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .Take(Math.Max(limit * 5, limit))
            .ToListAsync(cancellationToken);

        return candidateOrders
            .Where(order => order.Items.Any(item => catalogIdSet.Contains(item.CatalogId.Value)))
            .Take(limit)
            .ToList();
    }
}
