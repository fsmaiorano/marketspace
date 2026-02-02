using Basket.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json;

namespace Basket.Api.Infrastructure.Data;

public interface IBasketDbContext
{
    DbSet<ShoppingCartEntity> ShoppingCarts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class BasketDbContext : DbContext, IBasketDbContext
{
    public BasketDbContext(DbContextOptions<BasketDbContext> options)
        : base(options)
    {
    }

    public DbSet<ShoppingCartEntity> ShoppingCarts => Set<ShoppingCartEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Special handling for InMemory database - override the JSONB column type with a converter
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

