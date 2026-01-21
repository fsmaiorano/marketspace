using Basket.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Basket.Api.Infrastructure.Data.Configurations;

public class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCartEntity>
{
    public void Configure(EntityTypeBuilder<ShoppingCartEntity> builder)
    {
        builder.ToTable("ShoppingCarts");
        
        builder.HasKey(e => e.Username);
        
        builder.Property(e => e.Username)
            .HasMaxLength(255)
            .IsRequired();
        
        // Store Items as JSONB for PostgreSQL
        // Note: For InMemory database, the conversion is handled in the DbContext OnModelCreating
        builder.Property(e => e.Items)
            .IsRequired()
            .HasColumnType("jsonb");
        
        // TotalPrice is a computed property, not stored
        builder.Ignore(e => e.TotalPrice);
    }
}
