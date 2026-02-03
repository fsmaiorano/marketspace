using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using Microsoft.EntityFrameworkCore;
using Payment.Api.Domain.Entities;
using Payment.Api.Domain.Repositories;
using Payment.Api.Domain.ValueObjects;

namespace Payment.Api.Infrastructure.Data.Repositories;

public class PaymentRepository(IPaymentDbContext dbContext, IDomainEventDispatcher eventDispatcher) : IPaymentRepository
{
    public async Task<int> AddAsync(PaymentEntity payment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payment, nameof(payment));
        await dbContext.Payments.AddAsync(payment, cancellationToken);

        int result = await dbContext.SaveChangesAsync(cancellationToken);
        
        if(result <= 0) return result;

        await eventDispatcher.DispatchAsync(payment.DomainEvents, cancellationToken);
        payment.ClearDomainEvents();
        
        return result;
    }

    public async Task<int> UpdateAsync(PaymentEntity payment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payment, nameof(payment));

        bool exists = await dbContext.Payments
            .AsNoTracking()
            .AnyAsync(p => p.Id == payment.Id, cancellationToken);

        if (!exists)
            throw new InvalidOperationException($"Payment with ID {payment.Id} not found.");

        dbContext.Payments.Update(payment);
        int result = await dbContext.SaveChangesAsync(cancellationToken);

        if (result <= 0) return result;

        await eventDispatcher.DispatchAsync(payment.DomainEvents, cancellationToken);
        payment.ClearDomainEvents();

        return result;
    }

    public async Task<int> RemoveAsync(PaymentId id, CancellationToken cancellationToken = default)
    {
        PaymentEntity? storedEntity = await GetByIdAsync(id, cancellationToken: cancellationToken)
                                    ?? throw new InvalidOperationException($"Payment with ID {id} not found.");

        dbContext.Payments.Remove(storedEntity);
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PaymentEntity?> GetByIdAsync(PaymentId id, bool isTrackingEnabled = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<PaymentEntity> query = dbContext.Payments.AsQueryable();

        if (!isTrackingEnabled)
        {
            query = query.AsNoTracking();
        }

        return await query
            .Include(p => p.Attempts)
            .Include(p => p.Transactions)
            .Include(p => p.RiskAnalysis)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
