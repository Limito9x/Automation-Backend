namespace Automation.Pipeline.Domain.Entities;

public class WorkflowEdge : BaseEntity<Guid>
{
    public Guid WorkflowId { get; private set; }
    public Workflow Workflow { get; private set; } = null!;
    public Guid SourceWorkflowNodeId { get; private set; }
    public WorkflowNode SourceWorkflowNode { get; private set; } = null!;
    public string SourcePin { get; private set; } = string.Empty;
    public Guid TargetWorkflowNodeId { get; private set; }
    public WorkflowNode TargetWorkflowNode { get; private set; } = null!;
    public string TargetPin { get; private set; } = string.Empty;

    protected WorkflowEdge() { }

    public WorkflowEdge(
        Guid workflowId,
        Guid sourceWorkflowNodeId,
        string sourcePin,
        Guid targetWorkflowNodeId,
        string targetPin
    )
    {
        Id = Guid.NewGuid();
        WorkflowId = workflowId;
        SourceWorkflowNodeId = sourceWorkflowNodeId;
        SourcePin = sourcePin;
        TargetWorkflowNodeId = targetWorkflowNodeId;
        TargetPin = targetPin;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
