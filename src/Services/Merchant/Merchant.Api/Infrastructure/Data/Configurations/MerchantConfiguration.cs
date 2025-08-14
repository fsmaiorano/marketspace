using Merchant.Api.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Merchant.Api.Infrastructure.Data.Configurations;

public class MerchantConfiguration : IEntityTypeConfiguration<Merchant.Api.Domain.Entities.MerchantEntity>
{
    public void Configure(EntityTypeBuilder<Merchant.Api.Domain.Entities.MerchantEntity> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(c => c.Id)
            .HasConversion(merchantId => merchantId.Value,
                dbId => MerchantId.Of(dbId));
        
        builder.HasIndex(c => c.Id).IsUnique();
        
        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(m => m.Description)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(m => m.Address)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(m => m.PhoneNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.Email)
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(
                email => email.Value,
                dbEmail => Email.Of(dbEmail));
    }
}