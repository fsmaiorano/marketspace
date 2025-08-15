using Catalog.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Catalog.Api.Infrastructure.Data;

public interface ICatalogDbContext
{
    DbSet<CatalogEntity> Catalogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class CatalogDbContext : DbContext, ICatalogDbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<CatalogEntity> Catalogs => Set<CatalogEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}