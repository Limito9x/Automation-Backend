using System.Text.Json;
using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Domain.Entities;

public class WorkflowNode : BaseEntity<Guid>
{
    public Guid WorkflowId { get; private set; }
    public Workflow Workflow { get; private set; } = null!;

    public string RefId { get; private set; } = string.Empty;
    public WorkflowNodeKind Kind { get; private set; }
    public NodePosition Position { get; private set; } = new(0, 0);
    public JsonDocument? Config { get; private set; }

    protected WorkflowNode() { }

    public WorkflowNode(
        Guid id,
        Guid workflowId,
        string refId,
        WorkflowNodeKind kind,
        float positionX,
        float positionY,
        JsonDocument? config = null
    )
    {
        Id = id != Guid.Empty ? id : Guid.NewGuid();
        WorkflowId = workflowId;
        RefId = refId;
        Kind = kind;
        Position = new NodePosition(positionX, positionY);
        Config = config;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(float x, float y)
    {
        Position = new NodePosition(x, y);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateConfig(JsonDocument? config)
    {
        Config = config;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
