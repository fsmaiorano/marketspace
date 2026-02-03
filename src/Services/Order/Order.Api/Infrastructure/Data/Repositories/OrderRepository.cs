using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using Microsoft.EntityFrameworkCore;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Infrastructure.Data.Repositories;

public class OrderRepository(IOrderDbContext dbContext, IDomainEventDispatcher eventDispatcher) : IOrderRepository
{
    public async Task<int> AddAsync(OrderEntity order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order, nameof(order));
        await dbContext.Orders.AddAsync(order, cancellationToken);

        int result = await dbContext.SaveChangesAsync(cancellationToken);

        if (result <= 0) return result;

        await eventDispatcher.DispatchAsync(order.DomainEvents, cancellationToken);
        order.ClearDomainEvents();

        return result;
    }

    public async Task<int> UpdateAsync(OrderEntity order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order, nameof(order));

        OrderEntity storedEntity =
            await GetByIdAsync(order.Id, isTrackingEnabled: true, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException(
                $"Order with ID {order.Id} not found.");

        storedEntity.Update(
            order.ShippingAddress,
            order.BillingAddress,
            order.Payment,
            order.Status,
            order.Items);

        int result = await dbContext.SaveChangesAsync(cancellationToken);

        if (result <= 0) return result;

        await eventDispatcher.DispatchAsync(storedEntity.DomainEvents, cancellationToken);
        storedEntity.ClearDomainEvents();

        return result;
    }

    public async Task<int> RemoveAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        OrderEntity? storedEntity = await GetByIdAsync(id, cancellationToken: cancellationToken)
                                    ?? throw new InvalidOperationException(
                                        $"Order with ID {id} not found.");

        dbContext.Orders.Remove(storedEntity);
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrderEntity?> GetByIdAsync(OrderId id, bool isTrackingEnabled = true,
        CancellationToken cancellationToken = default)
    {
        if (isTrackingEnabled)
            return await dbContext.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(m => m.Id.Equals(id),
                    cancellationToken: cancellationToken);

        return await dbContext.Orders.AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(m => m.Id.Equals(id), cancellationToken);
    }
}