using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Api.Domain.Entities;
using Payment.Api.Domain.ValueObjects;

namespace Payment.Api.Infrastructure.Data.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransactionEntity>
{
    public void Configure(EntityTypeBuilder<PaymentTransactionEntity> builder)
    {
        builder.ToTable("PaymentTransactions");

        builder.HasKey(m => m.Id);
        builder.Property(o => o.Id)
            .HasConversion(orderId => orderId.Value,
                dbId => PaymentTransactionId.Of(dbId));

        builder.Property(t => t.PaymentId).IsRequired();

        builder.Property(t => t.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(t => t.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(t => t.ProviderTransactionId)
            .HasMaxLength(100);

        builder.Property(t => t.CreatedAt).IsRequired();
    }
}