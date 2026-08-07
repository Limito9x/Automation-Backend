using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Inspection.Infrastructure.Persistence.Configurations;

public class ContentTypeInspectorConfigConfiguration : IEntityTypeConfiguration<Domain.Entities.ContentTypeInspectorConfig>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.ContentTypeInspectorConfig> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.InspectorKey)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.RelevantFieldPath)
            .HasMaxLength(200);
            
        builder.Property(x => x.DisplayLabel)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.HasIndex(x => new { x.ContentTypeId, x.InspectorKey }).IsUnique();
    }
}
