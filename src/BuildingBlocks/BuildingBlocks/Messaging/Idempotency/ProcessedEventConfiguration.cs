using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Messaging.Idempotency;

public class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
    {
        builder.ToTable("ProcessedEvents");

        builder.HasKey(pe => pe.EventId);

        builder.Property(pe => pe.EventId)
            .ValueGeneratedNever();

        builder.Property(pe => pe.EventType)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(pe => pe.ProcessedAt)
            .IsRequired();
    }
}

