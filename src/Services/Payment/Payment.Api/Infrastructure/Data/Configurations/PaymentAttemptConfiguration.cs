using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Api.Domain.Entities;

namespace Payment.Api.Infrastructure.Data.Configurations;

public class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttemptEntity>
{
    public void Configure(EntityTypeBuilder<PaymentAttemptEntity> builder)
    {
        builder.ToTable("PaymentAttempts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).ValueGeneratedNever();

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

        builder.Property(a => a.CreatedAt).IsRequired();
    }
}

