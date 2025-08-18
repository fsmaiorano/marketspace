using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Enums;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(o => o.Id)
            .HasConversion(orderId => orderId.Value,
                dbId => OrderId.Of(dbId));

        builder.HasIndex(o => o.Id).IsUnique();

        builder.Property(o => o.CustomerId)
            .IsRequired()
            .HasConversion(customerId => customerId.Value,
                dbId => CustomerId.Of(dbId));

        builder.Property(o => o.ShippingAddress)
            .IsRequired()
            .HasConversion(
                address => address.ToString(),
                dbAddress => Address.FromString(dbAddress));
        
        builder.Property(o => o.BillingAddress)
            .IsRequired()
            .HasConversion(
                address => address.ToString(),
                dbAddress => Address.FromString(dbAddress));
        
        builder.Property(o => o.Payment)
            .IsRequired()
            .HasConversion(
                payment => payment.ToString(),
                dbPayment => Payment.FromString(dbPayment));

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion(
                status => status.ToString(),
                dbStatus => Enum.Parse<OrderStatusEnum>(dbStatus));

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(o => o.TotalAmount)
            .IsRequired()
            .HasConversion(
                price => price.Value,
                dbPrice => Price.Of(dbPrice));
    }
}