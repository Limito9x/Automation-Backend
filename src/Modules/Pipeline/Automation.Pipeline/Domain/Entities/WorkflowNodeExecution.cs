using System.Text.Json;
using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Domain.Entities;

public class WorkflowNodeExecution : BaseEntity<Guid>
{
    public Guid WorkflowExecutionId { get; private set; }
    public WorkflowExecution WorkflowExecution { get; private set; } = null!;
    public Guid WorkflowNodeId { get; private set; }
    public WorkflowNode WorkflowNode { get; private set; } = null!;
    public ExecutionStatus Status { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? FinishedAt { get; private set; }
    public JsonDocument? Output { get; private set; }
    public string? ErrorMessage { get; private set; }

    protected WorkflowNodeExecution() { }

    public WorkflowNodeExecution(
        Guid workflowExecutionId,
        Guid workflowNodeId,
        ExecutionStatus status = ExecutionStatus.Pending
    )
    {
        Id = Guid.NewGuid();
        WorkflowExecutionId = workflowExecutionId;
        WorkflowNodeId = workflowNodeId;
        Status = status;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkRunning()
    {
        Status = ExecutionStatus.Running;
        StartedAt = DateTimeOffset.UtcNow;
        FinishedAt = null;
        ErrorMessage = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkSucceeded(JsonDocument? output = null)
    {
        Status = ExecutionStatus.Succeeded;
        FinishedAt = DateTimeOffset.UtcNow;
        Output = output;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Status = ExecutionStatus.Failed;
        FinishedAt = DateTimeOffset.UtcNow;
        ErrorMessage = error;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
