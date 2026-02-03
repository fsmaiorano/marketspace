using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Api.Domain.Entities;

namespace Payment.Api.Infrastructure.Data.Configurations;

public class RiskAnalysisConfiguration : IEntityTypeConfiguration<RiskAnalysisEntity>
{
    public void Configure(EntityTypeBuilder<RiskAnalysisEntity> builder)
    {
        builder.ToTable("RiskAnalyses");

        builder.HasKey(r => r.PaymentId);

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
