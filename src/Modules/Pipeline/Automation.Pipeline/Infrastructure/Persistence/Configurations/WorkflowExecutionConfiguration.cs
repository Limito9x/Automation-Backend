using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Pipeline.Infrastructure.Persistence.Configurations;

public class WorkflowExecutionConfiguration : IEntityTypeConfiguration<Domain.Entities.WorkflowExecution>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.WorkflowExecution> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Workflow)
            .WithMany()
            .HasForeignKey(x => x.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.TriggerEventType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.TriggerPayload)
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.HasMany(x => x.NodeExecutions)
            .WithOne(x => x.WorkflowExecution)
            .HasForeignKey(x => x.WorkflowExecutionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
