using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Pipeline.Infrastructure.Persistence.Configurations;

public class NodeDefinitionConfiguration : IEntityTypeConfiguration<Domain.Entities.NodeDefinition>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.NodeDefinition> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Kind)
            .HasConversion<string>()
            .HasMaxLength(50);
            
        builder.HasIndex(x => x.RefId);
    }
}
