using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Resource.Infrastructure.Persistence.Configurations;

public class WorkspaceConfiguration : IEntityTypeConfiguration<Domain.Entities.Workspace>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Workspace> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Kind)
            .IsRequired();

        builder.Property(x => x.RootPath)
            .HasMaxLength(500);

        builder.HasMany(x => x.Resources)
            .WithOne(x => x.Workspace)
            .HasForeignKey(x => x.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.AgentId);
    }
}
