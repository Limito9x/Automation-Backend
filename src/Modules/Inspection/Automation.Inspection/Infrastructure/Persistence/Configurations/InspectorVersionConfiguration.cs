using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Inspection.Infrastructure.Persistence.Configurations;

public class InspectorVersionConfiguration
    : IEntityTypeConfiguration<Domain.Entities.InspectorVersion>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.InspectorVersion> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Version).IsRequired();

        builder.Property(x => x.EntryPoint).IsRequired().HasMaxLength(500);

        builder.Property(x => x.ScriptHash).HasMaxLength(64);

        builder
            .HasOne(x => x.Inspector)
            .WithMany(x => x.Versions)
            .HasForeignKey(x => x.InspectorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.InspectorId, x.Version }).IsUnique();
    }
}
