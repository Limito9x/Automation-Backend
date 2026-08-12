using System.Text.Json;
using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Domain.Entities;

public class NodeExecution : BaseEntity<Guid>
{
    public Guid PipelineExecutionId { get; private set; }
    public PipelineExecution PipelineExecution { get; private set; } = null!;
    public Guid PipelineNodeId { get; private set; }
    public PipelineNode PipelineNode { get; private set; } = null!;
    public ExecutionStatus Status { get; private set; }
    public JsonDocument Progress { get; private set; } = null!;

    protected NodeExecution() { }

    public NodeExecution(Guid pipelineExecutionId, Guid pipelineNodeId, JsonDocument progress)
    {
        Id = Guid.NewGuid();
        PipelineExecutionId = pipelineExecutionId;
        PipelineNodeId = pipelineNodeId;
        Status = ExecutionStatus.Pending;
        Progress = progress;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}

