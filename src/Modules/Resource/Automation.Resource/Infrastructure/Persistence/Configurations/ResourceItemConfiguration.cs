using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Resource.Infrastructure.Persistence.Configurations;

public class ResourceItemConfiguration : IEntityTypeConfiguration<Domain.Entities.ResourceItem>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.ResourceItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.FilePath)
            .HasMaxLength(500);

        builder.HasOne(x => x.Workspace)
            .WithMany(x => x.Resources)
            .HasForeignKey(x => x.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.PlatformExtensionId);
        builder.HasIndex(x => x.ContentId);
    }
}

