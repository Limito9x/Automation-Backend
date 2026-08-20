using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Domain.Entities;

public class NodeDefinition : BaseEntity<Guid>
{
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Key { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public List<PinDefinition> Inputs { get; private set; } = new();
    public List<PinDefinition> Outputs { get; private set; } = new();
    public string Executor { get; private set; } = string.Empty;

    protected NodeDefinition() { }

    public NodeDefinition(
        Guid projectId,
        string name,
        string key,
        string label,
        string executor,
        List<PinDefinition> inputs,
        List<PinDefinition> outputs
    )
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        Name = name;
        Key = key;
        Label = label;
        Executor = executor;
        Inputs = inputs;
        Outputs = outputs;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(
        string name,
        string label,
        string executor,
        List<PinDefinition> inputs,
        List<PinDefinition> outputs
    )
    {
        Name = name;
        Label = label;
        Executor = executor;
        Inputs = inputs;
        Outputs = outputs;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
