using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Inspection.Infrastructure.Persistence.Configurations;

public class InspectionConfiguration : IEntityTypeConfiguration<Domain.Entities.Inspection>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Inspection> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Version)
            .IsRequired();

        builder.Property(x => x.Data)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasOne(x => x.InspectorVersion)
            .WithMany()
            .HasForeignKey(x => x.InspectorVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ResourceId);
        builder.HasIndex(x => new { x.ResourceId, x.Version })
            .IsUnique();
    }
}
