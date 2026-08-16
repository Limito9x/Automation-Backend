using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Inspection.Infrastructure.Persistence.Configurations;

public class InspectionConfiguration : IEntityTypeConfiguration<Domain.Entities.Inspection>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Inspection> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Data)
            .HasColumnType("jsonb");

        builder.Property(x => x.SummaryMessage)
            .HasMaxLength(2000);

        builder.HasOne(x => x.InspectorVersion)
            .WithMany()
            .HasForeignKey(x => x.InspectorVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ResourceVersionId);
        builder.HasIndex(x => new { x.ResourceVersionId, x.InspectorVersionId });
    }
}
