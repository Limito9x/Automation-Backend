using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Domain.Entities;

public class NodeDefinition : BaseEntity<Guid>
{
    public NodeDefinitionKind Kind { get; private set; }
    public Guid RefId { get; private set; }

    protected NodeDefinition() { }

    public NodeDefinition(NodeDefinitionKind kind, Guid refId)
    {
        Id = Guid.NewGuid();
        Kind = kind;
        RefId = refId;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
