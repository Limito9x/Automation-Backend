using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Inspection.Infrastructure.Persistence.Configurations;

public class InspectorConfiguration : IEntityTypeConfiguration<Domain.Entities.Inspector>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Inspector> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ExecutorKey)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => new { x.ProjectId, x.Key })
            .IsUnique();
    }
}
