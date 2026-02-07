using BuildingBlocks.Messaging.Idempotency;
using BuildingBlocks.Messaging.Outbox;
using Catalog.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Catalog.Api.Infrastructure.Data;

public interface ICatalogDbContext
{
    DbSet<CatalogEntity> Catalogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class CatalogDbContext : DbContext, ICatalogDbContext, IOutboxDbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<CatalogEntity> Catalogs => Set<CatalogEntity>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.ApplyConfiguration(new OutboxMessageEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedEventConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}