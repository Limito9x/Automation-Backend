using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Pipeline.Infrastructure.Persistence.Configurations;

public class ToolDefinitionConfiguration : IEntityTypeConfiguration<Domain.Entities.ToolDefinition>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.ToolDefinition> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(x => x.InputPins)
            .HasColumnType("jsonb")
            .IsRequired();
            
        builder.Property(x => x.OutputPins)
            .HasColumnType("jsonb")
            .IsRequired();
            
        builder.Property(x => x.HandlerKey)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.HasIndex(x => x.Key).IsUnique();
    }
}
