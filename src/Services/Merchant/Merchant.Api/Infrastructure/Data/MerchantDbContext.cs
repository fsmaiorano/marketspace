using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Merchant.Api.Infrastructure.Data;

public interface IMerchantDbContext
{
    DbSet<Merchant.Api.Domain.Entities.MerchantEntity> Merchants { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class MerchantDbContext : DbContext, IMerchantDbContext
{
    public MerchantDbContext(DbContextOptions<MerchantDbContext> options)
        : base(options)
    {
    }

    public DbSet<Merchant.Api.Domain.Entities.MerchantEntity> Merchants => Set<Merchant.Api.Domain.Entities.MerchantEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}