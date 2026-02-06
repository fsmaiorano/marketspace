using System.Reflection;
using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Merchant.Api.Infrastructure.Data;

public interface IMerchantDbContext
{
    DbSet<Merchant.Api.Domain.Entities.MerchantEntity> Merchants { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class MerchantDbContext : DbContext, IMerchantDbContext, IOutboxDbContext
{
    public MerchantDbContext(DbContextOptions<MerchantDbContext> options)
        : base(options)
    {
    }

    public DbSet<Merchant.Api.Domain.Entities.MerchantEntity> Merchants => Set<Merchant.Api.Domain.Entities.MerchantEntity>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.ApplyConfiguration(new OutboxMessageEntityTypeConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}