using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Pipeline.Infrastructure.Persistence.Configurations;

public class PipelineNodeConfiguration : IEntityTypeConfiguration<Domain.Entities.PipelineNode>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.PipelineNode> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.Pipeline)
            .WithMany()
            .HasForeignKey(x => x.PipelineId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(x => x.NodeDefinition)
            .WithMany()
            .HasForeignKey(x => x.NodeDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
