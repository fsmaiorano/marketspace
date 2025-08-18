using BuildingBlocks.Abstractions;
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
            .IsRequired();

        builder.Property(o => o.ShippingAddress)
            .IsRequired()
            .HasConversion(
                address => address,
                dbAddress => Address.Of(dbAddress.FirstName,
                    dbAddress.LastName,
                    dbAddress.EmailAddress,
                    dbAddress.AddressLine,
                    dbAddress.Country,
                    dbAddress.State,
                    dbAddress.ZipCode));

        builder.Property(o => o.BillingAddress)
            .IsRequired()
            .HasConversion(
                address => address,
                dbAddress => Address.Of(dbAddress.FirstName,
                    dbAddress.LastName,
                    dbAddress.EmailAddress,
                    dbAddress.AddressLine,
                    dbAddress.Country,
                    dbAddress.State,
                    dbAddress.ZipCode));

        builder.Property(o => o.Payment)
            .IsRequired()
            .HasConversion(
                payment => payment,
                dbPayment => Payment.Of(dbPayment.CardNumber,
                    dbPayment.CardName,
                    dbPayment.Expiration,
                    dbPayment.Cvv,
                    dbPayment.PaymentMethod));

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