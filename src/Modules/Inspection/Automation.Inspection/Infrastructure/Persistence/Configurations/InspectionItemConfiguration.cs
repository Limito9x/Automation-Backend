using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Inspection.Infrastructure.Persistence.Configurations;

public class InspectionItemConfiguration : IEntityTypeConfiguration<Domain.Entities.InspectionItem>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.InspectionItem> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.Inspection)
            .WithMany()
            .HasForeignKey(x => x.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(x => x.RawData)
            .HasColumnType("jsonb")
            .IsRequired();
            
        builder.HasIndex(x => new { x.InspectionId, x.Name });
    }
}
