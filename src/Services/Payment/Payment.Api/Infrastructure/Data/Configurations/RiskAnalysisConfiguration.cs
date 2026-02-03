using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Api.Domain.Entities;
using Payment.Api.Domain.ValueObjects;

namespace Payment.Api.Infrastructure.Data.Configurations;

public class RiskAnalysisConfiguration : IEntityTypeConfiguration<RiskAnalysisEntity>
{
    public void Configure(EntityTypeBuilder<RiskAnalysisEntity> builder)
    {
        builder.ToTable("RiskAnalyses");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(
                riskAnalysisId => riskAnalysisId.Value,
                dbId => RiskAnalysisId.Of(dbId));

        builder.Property(r => r.PaymentId)
            .HasConversion(
                paymentId => paymentId.Value,
                dbId => PaymentId.Of(dbId))
            .IsRequired();

        builder.Property(r => r.IpAddress)
            .HasMaxLength(50);

        builder.Property(r => r.Country)
            .HasMaxLength(50);

        builder.Property(r => r.Score);

        builder.Property(r => r.Decision)
            .HasConversion<string>()
            .IsRequired();
    }
}
