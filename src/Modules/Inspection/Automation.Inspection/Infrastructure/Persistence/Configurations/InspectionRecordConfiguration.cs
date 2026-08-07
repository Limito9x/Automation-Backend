using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Inspection.Infrastructure.Persistence.Configurations;

public class InspectionRecordConfiguration : IEntityTypeConfiguration<Domain.Entities.InspectionRecord>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.InspectionRecord> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.InspectorKey)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.ResultJson)
            .HasColumnType("jsonb")
            .IsRequired();
            
        builder.HasIndex(x => x.ResourceVersionId);
    }
}
