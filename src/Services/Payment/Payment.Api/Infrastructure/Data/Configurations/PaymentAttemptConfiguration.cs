using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Api.Domain.Entities;
using Payment.Api.Domain.ValueObjects;

namespace Payment.Api.Infrastructure.Data.Configurations;

public class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttemptEntity>
{
    public void Configure(EntityTypeBuilder<PaymentAttemptEntity> builder)
    {
        builder.ToTable("PaymentAttempts");

        builder.HasKey(m => m.Id);
        builder.Property(o => o.Id)
            .HasConversion(orderId => orderId.Value,
                dbId => PaymentAttemptId.Of(dbId));

        builder.Property(a => a.PaymentId).IsRequired();

        builder.Property(a => a.AttemptNumber).IsRequired();

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(a => a.StatusDetail)
            .HasMaxLength(200);

        builder.Property(a => a.ProviderTransactionId)
            .HasMaxLength(100);

        builder.Property(a => a.ResponseCode)
            .HasMaxLength(50);

        builder.Property(a => a.ResponseMessage)
            .HasMaxLength(255);
    }
}