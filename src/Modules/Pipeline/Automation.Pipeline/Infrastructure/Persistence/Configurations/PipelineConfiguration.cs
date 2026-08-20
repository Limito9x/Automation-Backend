using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Pipeline.Infrastructure.Persistence.Configurations;

public class PipelineConfiguration : IEntityTypeConfiguration<Domain.Entities.Pipeline>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Pipeline> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);

        builder.HasIndex(x => new { x.ProjectId, x.Name }).IsUnique();
    }
}
