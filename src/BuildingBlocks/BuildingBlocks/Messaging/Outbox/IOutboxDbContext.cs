using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Messaging.Outbox;

public interface IOutboxDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; }
}

