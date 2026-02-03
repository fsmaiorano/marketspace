using Microsoft.EntityFrameworkCore;
using Payment.Api.Domain.Entities;
using System.Reflection;

namespace Payment.Api.Infrastructure.Data;

public interface IPaymentDbContext
{
    DbSet<PaymentEntity> Payments { get; }
    DbSet<PaymentAttemptEntity> PaymentAttempts { get; }
    DbSet<PaymentTransactionEntity> PaymentTransactions { get; }
    DbSet<RiskAnalysisEntity> RiskAnalysis { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class PaymentDbContext : DbContext, IPaymentDbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();
    public DbSet<PaymentAttemptEntity> PaymentAttempts => Set<PaymentAttemptEntity>();
    public DbSet<PaymentTransactionEntity> PaymentTransactions => Set<PaymentTransactionEntity>();
    public DbSet<RiskAnalysisEntity> RiskAnalysis => Set<RiskAnalysisEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}