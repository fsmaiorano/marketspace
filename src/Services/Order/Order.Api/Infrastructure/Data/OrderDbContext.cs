using BuildingBlocks.Messaging.Idempotency;
using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Order.Api.Domain.Entities;
using System.Reflection;

namespace Order.Api.Infrastructure.Data;

public interface IOrderDbContext
{
    DbSet<OrderEntity> Orders { get; }
    DbSet<OrderItemEntity> OrderItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class OrderDbContext : DbContext, IOrderDbContext, IOutboxDbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.ApplyConfiguration(new OutboxMessageEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedEventConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}