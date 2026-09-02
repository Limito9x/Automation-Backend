using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Pipeline.Infrastructure.Persistence.Configurations;

public class WorkflowNodeExecutionConfiguration : IEntityTypeConfiguration<Domain.Entities.WorkflowNodeExecution>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.WorkflowNodeExecution> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.WorkflowNode)
            .WithMany()
            .HasForeignKey(x => x.WorkflowNodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Output)
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000)
            .IsRequired(false);
    }
}
