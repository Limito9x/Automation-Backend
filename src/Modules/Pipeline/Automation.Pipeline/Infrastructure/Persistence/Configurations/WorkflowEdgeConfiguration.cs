using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Pipeline.Infrastructure.Persistence.Configurations;

public class WorkflowEdgeConfiguration : IEntityTypeConfiguration<Domain.Entities.WorkflowEdge>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.WorkflowEdge> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Workflow)
            .WithMany(x => x.Edges)
            .HasForeignKey(x => x.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SourceWorkflowNode)
            .WithMany()
            .HasForeignKey(x => x.SourceWorkflowNodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.TargetWorkflowNode)
            .WithMany()
            .HasForeignKey(x => x.TargetWorkflowNodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.SourcePin)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.TargetPin)
            .IsRequired()
            .HasMaxLength(100);
    }
}
