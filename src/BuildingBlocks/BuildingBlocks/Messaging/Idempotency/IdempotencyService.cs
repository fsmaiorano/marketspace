using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BuildingBlocks.Messaging.Idempotency;

public class IdempotencyService<TContext> : IIdempotencyService where TContext : DbContext
{
    private readonly TContext _context;

    public IdempotencyService(TContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(Guid eventId, string eventName, Func<CancellationToken, Task> handler, CancellationToken cancellationToken = default)
    {
        // Use ExecutionStrategy to handle transient failures and ensure transaction commit
        IExecutionStrategy strategy = _context.Database.CreateExecutionStrategy();
        
        await strategy.ExecuteAsync(async () =>
        {
            _context.ChangeTracker.Clear();

            await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            bool exists = await _context.Set<ProcessedEvent>()
                .AnyAsync(pe => pe.EventId == eventId, cancellationToken);

            if (exists)
            {
                // Already processed, just return
                await transaction.RollbackAsync(cancellationToken);
                return;
            }

            await handler(cancellationToken);

            ProcessedEvent processedEvent = new ProcessedEvent
            {
                EventId = eventId,
                ProcessedAt = DateTime.UtcNow,
                EventType = eventName
            };

            await _context.Set<ProcessedEvent>().AddAsync(processedEvent, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });
    }
}


