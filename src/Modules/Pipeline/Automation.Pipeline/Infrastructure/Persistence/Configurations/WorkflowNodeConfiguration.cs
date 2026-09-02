using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Pipeline.Infrastructure.Persistence.Configurations;

public class WorkflowNodeConfiguration : IEntityTypeConfiguration<Domain.Entities.WorkflowNode>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.WorkflowNode> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Workflow)
            .WithMany(x => x.Nodes)
            .HasForeignKey(x => x.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.RefId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Kind)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.ComplexProperty(x => x.Position);

        builder.Property(x => x.Config)
            .HasColumnType("jsonb");
    }
}
