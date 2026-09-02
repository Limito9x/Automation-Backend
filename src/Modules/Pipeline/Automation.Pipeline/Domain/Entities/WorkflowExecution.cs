using System.Text.Json;
using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Domain.Entities;

public class WorkflowExecution : BaseEntity<Guid>
{
    public Guid WorkflowId { get; private set; }
    public Workflow Workflow { get; private set; } = null!;
    public WorkflowEventType TriggerEventType { get; private set; }
    public JsonDocument? TriggerPayload { get; private set; }
    public ExecutionStatus Status { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? FinishedAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    private readonly List<WorkflowNodeExecution> _nodeExecutions = new();
    public IReadOnlyList<WorkflowNodeExecution> NodeExecutions => _nodeExecutions;

    protected WorkflowExecution() { }

    public WorkflowExecution(
        Guid workflowId,
        WorkflowEventType triggerEventType,
        JsonDocument? triggerPayload = null
    )
    {
        Id = Guid.NewGuid();
        WorkflowId = workflowId;
        TriggerEventType = triggerEventType;
        TriggerPayload = triggerPayload;
        Status = ExecutionStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Start()
    {
        Status = ExecutionStatus.Running;
        StartedAt = DateTimeOffset.UtcNow;
        FinishedAt = null;
        ErrorMessage = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkSucceeded()
    {
        Status = ExecutionStatus.Succeeded;
        FinishedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Status = ExecutionStatus.Failed;
        FinishedAt = DateTimeOffset.UtcNow;
        ErrorMessage = error;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkCancelled()
    {
        Status = ExecutionStatus.Cancelled;
        FinishedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddNodeExecution(WorkflowNodeExecution nodeExecution)
    {
        _nodeExecutions.Add(nodeExecution);
    }
}
