using System.Text.Json;
using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Domain.Entities;

public class Workflow : BaseEntity<Guid>
{
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<WorkflowNode> _nodes = new();
    public IReadOnlyList<WorkflowNode> Nodes => _nodes;

    private readonly List<WorkflowEdge> _edges = new();
    public IReadOnlyList<WorkflowEdge> Edges => _edges;

    protected Workflow() { }

    public Workflow(Guid projectId, string name, string? description = null, bool isActive = true)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        Name = name;
        Description = description;
        IsActive = isActive;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddNode(string refId, WorkflowNodeKind kind, float x, float y, JsonDocument? config = null)
    {
        _nodes.Add(new WorkflowNode(Guid.NewGuid(), Id, refId, kind, x, y, config));
    }

    public void AddNode(WorkflowNode node)
    {
        _nodes.Add(node);
    }

    public void RemoveNode(Guid nodeId)
    {
        var node = _nodes.FirstOrDefault(x => x.Id == nodeId);
        if (node != null)
        {
            _nodes.Remove(node);
        }
    }

    public void UpdateNode(Guid nodeId, float x, float y)
    {
        var node = _nodes.FirstOrDefault(x => x.Id == nodeId);
        node?.Update(x, y);
    }

    public void UpdateNodeConfig(Guid nodeId, JsonDocument? config)
    {
        var node = _nodes.FirstOrDefault(x => x.Id == nodeId);
        node?.UpdateConfig(config);
    }

    public void AddEdge(
        Guid sourceWorkflowNodeId,
        string sourcePin,
        Guid targetWorkflowNodeId,
        string targetPin
    )
    {
        _edges.Add(
            new WorkflowEdge(Id, sourceWorkflowNodeId, sourcePin, targetWorkflowNodeId, targetPin)
        );
    }

    public void RemoveEdge(Guid edgeId)
    {
        var edge = _edges.FirstOrDefault(x => x.Id == edgeId);
        if (edge != null)
        {
            _edges.Remove(edge);
        }
    }

    public void ClearEdges()
    {
        _edges.Clear();
    }
}
