using Microsoft.EntityFrameworkCore;
using Payment.Api.Domain.Entities;
using Payment.Api.Domain.Enums;
using Payment.Api.Domain.Repositories;
using Payment.Api.Domain.ValueObjects;

namespace Payment.Api.Infrastructure.Data.Repositories;

public class PaymentRepository(IPaymentDbContext dbContext) : IPaymentRepository
{
    public async Task<int> AddAsync(PaymentEntity payment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payment);
        await dbContext.Payments.AddAsync(payment, cancellationToken);

        int result = await dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<int> UpdateAsync(PaymentEntity payment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payment);

        bool exists = await dbContext.Payments
            .AsNoTracking()
            .AnyAsync(p => p.Id == payment.Id, cancellationToken);

        if (!exists)
            throw new InvalidOperationException($"Payment with ID {payment.Id} not found.");

        dbContext.Payments.Update(payment);
        int result = await dbContext.SaveChangesAsync(cancellationToken);


        return result;
    }

    public async Task<int> RemoveAsync(PaymentId id, CancellationToken cancellationToken = default)
    {
        PaymentEntity storedEntity = await GetByIdAsync(id, isTrackingEnabled: true, cancellationToken)
                                     ?? throw new InvalidOperationException($"Payment with ID {id} not found.");

        dbContext.Payments.Remove(storedEntity);
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PaymentEntity?> GetByIdAsync(PaymentId id, bool isTrackingEnabled = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<PaymentEntity> query = dbContext.Payments;

        if (!isTrackingEnabled)
            query = query.AsNoTracking();

        return await query
            .Include(p => p.Attempts)
            .Include(p => p.Transactions)
            .Include(p => p.RiskAnalysis)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<PaymentEntity>> GetByStatus(PaymentStatusEnum status, bool isTrackingEnabled = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<PaymentEntity> query = dbContext.Payments.Where(p => p.Status == status);
        
        if (!isTrackingEnabled)
            query = query.AsNoTracking();

        return await query.Where(p => p.Status == status).ToListAsync(cancellationToken);
    }

    public Task<IEnumerable<PaymentEntity>> GetAllCreatedPaymentsAsync(bool isTrackingEnabled = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<PaymentEntity> query = dbContext.Payments.Where(p => p.Status == PaymentStatusEnum.Created);

        if (!isTrackingEnabled)
            query = query.AsNoTracking();

        return query.Where(p => p.Status == PaymentStatusEnum.Created).ToListAsync(cancellationToken)
            .ContinueWith(t => (IEnumerable<PaymentEntity>)t.Result, cancellationToken);
    }

    public async Task<int> PatchStatusAsync(PaymentEntity payment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payment);

        bool exists = await dbContext.Payments
            .AsNoTracking()
            .AnyAsync(p => p.Id == payment.Id, cancellationToken);

        if (!exists)
            throw new InvalidOperationException($"Payment with ID {payment.Id} not found.");

        payment.PatchStatus(payment.Status);
        dbContext.Payments.Update(payment);
        int result = await dbContext.SaveChangesAsync(cancellationToken);
        
        return result;
    }
}