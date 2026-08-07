using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Domain.Entities;

public class PipelineExecution : BaseEntity<Guid>
{
    public Guid PipelineId { get; private set; }
    public PipelineItem Pipeline { get; private set; } = null!;
    public ExecutionStatus Status { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? FinishedAt { get; private set; }

    protected PipelineExecution() { }

    public PipelineExecution(Guid pipelineId)
    {
        Id = Guid.NewGuid();
        PipelineId = pipelineId;
        Status = ExecutionStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
