using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Api.Domain.Entities;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItemEntity>
{
    public void Configure(EntityTypeBuilder<OrderItemEntity> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(o => o.Id)
            .HasConversion(orderItemId => orderItemId.Value,
                dbId => OrderItemId.Of(dbId));

        builder.HasIndex(o => o.Id).IsUnique();

        builder.Property(o => o.OrderId)
            .IsRequired()
            .HasConversion(orderId => orderId.Value,
                dbId => OrderId.Of(dbId));

        builder.Property(o => o.CatalogId)
            .IsRequired()
            .HasConversion(catalogId => catalogId.Value,
                dbId => CatalogId.Of(dbId));

        builder.Property(o => o.Quantity)
            .IsRequired();

        builder.Property(o => o.Price)
            .IsRequired()
            .HasConversion(
                price => price.Value,
                dbPrice => Price.Of(dbPrice));
    }
}