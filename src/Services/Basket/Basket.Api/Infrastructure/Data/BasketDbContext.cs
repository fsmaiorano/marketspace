using Basket.Api.Domain.Entities;
using BuildingBlocks.Messaging.Outbox;
using BuildingBlocks.Messaging.Idempotency;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json;

namespace Basket.Api.Infrastructure.Data;

public interface IBasketDbContext
{
    DbSet<ShoppingCartEntity> ShoppingCarts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class BasketDbContext : DbContext, IBasketDbContext, IOutboxDbContext
{
    public BasketDbContext(DbContextOptions<BasketDbContext> options)
        : base(options)
    {
    }

    public DbSet<ShoppingCartEntity> ShoppingCarts => Set<ShoppingCartEntity>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.ApplyConfiguration(new OutboxMessageEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedEventConfiguration());

        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            modelBuilder.Entity<ShoppingCartEntity>()
                .Property(e => e.Items)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<ShoppingCartItemEntity>>(v, (JsonSerializerOptions?)null) ?? new List<ShoppingCartItemEntity>()
                );
        }

        base.OnModelCreating(modelBuilder);
    }
}
