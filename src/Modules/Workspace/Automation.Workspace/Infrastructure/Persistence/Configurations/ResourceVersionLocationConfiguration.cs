using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Workspace.Infrastructure.Persistence.Configurations;

public class ResourceVersionLocationConfiguration : IEntityTypeConfiguration<Domain.Entities.ResourceVersionLocation>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.ResourceVersionLocation> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RelativePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(x => x.ResourceVersion)
            .WithMany(x => x.Locations)
            .HasForeignKey(x => x.ResourceVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.WorkspaceAgent)
            .WithMany(x => x.Locations)
            .HasForeignKey(x => x.WorkspaceAgentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.WorkspaceAgentId, x.RelativePath })
            .IsUnique();
    }
}
