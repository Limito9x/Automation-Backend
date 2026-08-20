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
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? FinishedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public JsonDocument? Output { get; private set; }
    public JsonDocument? Log { get; private set; }
    public JsonDocument? Progress { get; private set; }

    protected NodeExecution() { }

    public NodeExecution(
        Guid pipelineExecutionId,
        Guid pipelineNodeId,
        JsonDocument? progress = null,
        ExecutionStatus status = ExecutionStatus.Pending
    )
    {
        Id = Guid.NewGuid();
        PipelineExecutionId = pipelineExecutionId;
        PipelineNodeId = pipelineNodeId;
        Status = status;
        Progress = progress;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkRunning()
    {
        Status = ExecutionStatus.Running;
        StartedAt = DateTimeOffset.UtcNow;
        FinishedAt = null;
        ErrorMessage = null;
    }

    public void MarkSucceeded(JsonDocument output, JsonDocument? log = null)
    {
        Status = ExecutionStatus.Succeeded;
        FinishedAt = DateTimeOffset.UtcNow;
        Output = output;
        Log = log;
    }

    public void MarkFailed(string error, JsonDocument? log = null)
    {
        Status = ExecutionStatus.Failed;
        FinishedAt = DateTimeOffset.UtcNow;
        ErrorMessage = error;
        Log = log;
    }

    public void UpdateProgress(JsonDocument progress)
    {
        Progress = progress;
    }
}
