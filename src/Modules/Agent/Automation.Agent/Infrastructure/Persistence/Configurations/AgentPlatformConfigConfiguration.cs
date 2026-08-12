using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Automation.Agent.Domain.Entities;

namespace Automation.Agent.Infrastructure.Persistence.Configurations;

internal class AgentPlatformConfigConfiguration : IEntityTypeConfiguration<AgentPlatformConfig>
{
    public void Configure(EntityTypeBuilder<AgentPlatformConfig> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExecutablePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Version)
            .HasMaxLength(50);

        builder.HasIndex(x => new { x.AgentId, x.PlatformId })
            .IsUnique();
    }
}
