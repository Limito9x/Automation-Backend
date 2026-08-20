using System.Text.Json;
using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Domain.Entities;

public class PipelineExecution : BaseEntity<Guid>
{
    public Guid PipelineId { get; private set; }
    public Pipeline Pipeline { get; private set; } = null!;
    public Guid AgentId { get; private set; }
    public ExecutionStatus Status { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? FinishedAt { get; private set; }
    public JsonDocument? ExecutionState { get; private set; }
    public int NextNodeIndex { get; private set; }
    public string? CurrentBatchId { get; private set; }
    public string? ErrorMessage { get; private set; }

    protected PipelineExecution() { }

    public PipelineExecution(Guid pipelineId, Guid agentId)
    {
        Id = Guid.NewGuid();
        PipelineId = pipelineId;
        AgentId = agentId;
        Status = ExecutionStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
        NextNodeIndex = 0;
    }

    public void Start()
    {
        Status = ExecutionStatus.Running;
        StartedAt = DateTimeOffset.UtcNow;
        FinishedAt = null;
        ErrorMessage = null;
    }

    public void SetState(JsonDocument state, int nextIndex, string? batchId = null)
    {
        ExecutionState = state;
        NextNodeIndex = nextIndex;
        CurrentBatchId = batchId;
    }

    public void MarkWaitingForAgent(string batchId, int nextIndex, JsonDocument state)
    {
        Status = ExecutionStatus.WaitingForAgent;
        CurrentBatchId = batchId;
        NextNodeIndex = nextIndex;
        ExecutionState = state;
    }

    public void MarkSucceeded(JsonDocument state)
    {
        Status = ExecutionStatus.Succeeded;
        FinishedAt = DateTimeOffset.UtcNow;
        ExecutionState = state;
        CurrentBatchId = null;
    }

    public void MarkFailed(string error, JsonDocument? state = null)
    {
        Status = ExecutionStatus.Failed;
        FinishedAt = DateTimeOffset.UtcNow;
        ErrorMessage = error;
        if (state != null)
        {
            ExecutionState = state;
        }
    }

    public void MarkCancelled()
    {
        Status = ExecutionStatus.Cancelled;
        FinishedAt = DateTimeOffset.UtcNow;
    }
}
