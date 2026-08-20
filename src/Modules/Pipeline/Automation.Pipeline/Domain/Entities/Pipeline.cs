namespace Automation.Pipeline.Domain.Entities;

public class Pipeline : BaseEntity<Guid>
{
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    private readonly List<PipelineNode> _nodes = new();
    public IReadOnlyList<PipelineNode> Nodes => _nodes;
    private readonly List<PipelineEdge> _edges = new();
    public IReadOnlyList<PipelineEdge> Edges => _edges;
    private readonly List<PipelineInput> _inputs = new();
    public IReadOnlyList<PipelineInput> Inputs => _inputs;

    protected Pipeline() { }

    public Pipeline(Guid projectId, string name)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        Name = name;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void AddInput(PipelineInput input)
    {
        _inputs.Add(input);
    }

    public void RemoveInput(Guid inputId)
    {
        var input = _inputs.FirstOrDefault(x => x.Id == inputId);
        if (input != null)
        {
            _inputs.Remove(input);
        }
    }

    public void AddNode(string refId, string kind, float x, float y, System.Text.Json.JsonDocument? config = null)
    {
        _nodes.Add(new PipelineNode(Guid.NewGuid(), Id, refId, kind, x, y, config));
    }

    public void AddNode(PipelineNode node)
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

    public void UpdateNodeConfig(Guid nodeId, System.Text.Json.JsonDocument? config)
    {
        var node = _nodes.FirstOrDefault(x => x.Id == nodeId);
        node?.UpdateConfig(config);
    }

    public void AddEdge(
        Guid sourcePipelineNodeId,
        string sourcePin,
        Guid targetPipelineNodeId,
        string targetPin
    )
    {
        _edges.Add(
            new PipelineEdge(Id, sourcePipelineNodeId, sourcePin, targetPipelineNodeId, targetPin)
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
