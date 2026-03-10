using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Api.Domain.Entities;
using Payment.Api.Domain.ValueObjects;
using System.Text.Json;

namespace Payment.Api.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<PaymentEntity>
{
    public void Configure(EntityTypeBuilder<PaymentEntity> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(m => m.Id);
        builder.Property(o => o.Id)
            .HasConversion(orderId => orderId.Value,
                dbId => PaymentId.Of(dbId));

        builder.Property(p => p.OrderId).IsRequired();

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.StatusDetail)
            .HasMaxLength(200);

        builder.Property(p => p.Method)
            .HasConversion(
                method => method.Value,
                dbValue => PaymentMethod.Of(dbValue))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Provider)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.ProviderTransactionId)
            .HasMaxLength(100);

        builder.Property(p => p.AuthorizationCode)
            .HasMaxLength(50);

        builder.Property(p => p.Items)
            .HasColumnType("jsonb")
            .HasConversion(
                items => JsonSerializer.Serialize(items, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<List<BuildingBlocks.Messaging.IntegrationEvents.OrderItemData>>(json,
                    (JsonSerializerOptions?)null) ?? new())
            .IsRequired()
            .HasDefaultValueSql("'[]'::jsonb")
            .Metadata.SetValueComparer(
                new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<
                    List<BuildingBlocks.Messaging.IntegrationEvents.OrderItemData>>(
                    (c1, c2) => JsonSerializer.Serialize(c1, (JsonSerializerOptions?)null) ==
                                JsonSerializer.Serialize(c2, (JsonSerializerOptions?)null),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasMany(p => p.Attempts)
            .WithOne()
            .HasForeignKey(a => a.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Transactions)
            .WithOne()
            .HasForeignKey(t => t.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.RiskAnalysis)
            .WithOne()
            .HasForeignKey<RiskAnalysisEntity>(r => r.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}