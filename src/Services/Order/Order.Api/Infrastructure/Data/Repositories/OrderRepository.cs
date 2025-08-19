using Microsoft.EntityFrameworkCore;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Enums;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Infrastructure.Data.Repositories;

public class OrderRepository(IOrderDbContext dbContext) : IOrderRepository
{
    public async Task<int> AddAsync(OrderEntity order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order, nameof(order));
        await dbContext.Orders.AddAsync(order, cancellationToken);
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> UpdateAsync(OrderEntity order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order, nameof(order));

        OrderEntity storedEntity = await GetByIdAsync(order.Id, cancellationToken: cancellationToken)
                                   ?? throw new InvalidOperationException(
                                       $"Order with ID {order.Id} not found.");

        OrderEntity updatedEntity = OrderEntity.Update(
            order.Id,
            order.CustomerId,
            order.ShippingAddress,
            order.BillingAddress,
            order.Payment,
            order.Status,
            order.Items.Count > 0 ? order.Items : storedEntity.Items);

        dbContext.Orders.Update(updatedEntity);
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<int> RemoveAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<OrderEntity?> GetByIdAsync(OrderId id, bool isTrackingEnabled = true,
        CancellationToken cancellationToken = default)
    {
        if (isTrackingEnabled)
            return await dbContext.Orders.FirstOrDefaultAsync(m => m.Id.Equals(id),
                cancellationToken: cancellationToken);

        return await dbContext.Orders.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id.Equals(id), cancellationToken);
    }
}