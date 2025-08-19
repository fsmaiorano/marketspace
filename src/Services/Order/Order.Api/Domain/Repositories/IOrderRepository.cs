using Order.Api.Domain.Entities;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Domain.Repositories;

public interface IOrderRepository
{
    Task<int> AddAsync(OrderEntity order, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(OrderEntity order, CancellationToken cancellationToken = default);
    Task<int> RemoveAsync(OrderId id, CancellationToken cancellationToken = default);

    Task<OrderEntity?> GetByIdAsync(OrderId id, bool isTrackingEnabled = true,
        CancellationToken cancellationToken = default);
}