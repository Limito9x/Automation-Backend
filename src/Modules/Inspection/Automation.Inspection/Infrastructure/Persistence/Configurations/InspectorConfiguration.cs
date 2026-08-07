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
            
        builder.Property(x => x.PlatformKey)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.SupportedExtension)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(x => x.ScriptPath)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(x => x.PrimaryFieldPath)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.HasIndex(x => x.Key).IsUnique();
    }
}
