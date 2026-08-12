using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Inspection.Infrastructure.Persistence.Configurations;

public class InspectorRuleConfiguration : IEntityTypeConfiguration<Domain.Entities.InspectorRule>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.InspectorRule> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Inspector)
            .WithMany()
            .HasForeignKey(x => x.InspectorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.PlatformExtensionId);
        builder.HasIndex(x => x.ContentTypeId);
    }
}
