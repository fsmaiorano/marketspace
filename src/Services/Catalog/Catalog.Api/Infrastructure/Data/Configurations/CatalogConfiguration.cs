using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Api.Infrastructure.Data.Configurations;

public class CatalogConfiguration : IEntityTypeConfiguration<CatalogEntity>
{
    public void Configure(EntityTypeBuilder<CatalogEntity> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(c => c.Id)
            .HasConversion(catalogId => catalogId.Value,
                dbId => CatalogId.Of(dbId));

        builder.HasIndex(c => c.Id).IsUnique();

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Price)
            .IsRequired()
            .HasConversion(
                price => price.Value,
                dbPrice => Price.Of(dbPrice));

        builder.Property(c => c.Categories)
            .HasConversion(
                categories => string.Join(',', categories),
                dbCategories => dbCategories.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));
        
        builder.Property(c => c.Categories)
            .HasMaxLength(500);

        builder.Property(m => m.MerchantId)
            .IsRequired();
    }
}