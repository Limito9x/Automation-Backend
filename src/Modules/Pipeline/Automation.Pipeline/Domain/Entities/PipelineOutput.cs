using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Domain.Entities;

public class PipelineOutput : BaseEntity<Guid>
{
    public Guid PipelineId { get; private set; }
    public Pipeline Pipeline { get; private set; } = null!;
    public string Key { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public PinPrimitiveType Type { get; private set; }
    public PinCardinality Cardinality { get; private set; } = PinCardinality.Single;
    public int Order { get; private set; }

    protected PipelineOutput() { }

    public PipelineOutput(
        Guid pipelineId,
        string key,
        string label,
        PinPrimitiveType type,
        PinCardinality cardinality = PinCardinality.Single,
        int order = 0
    )
    {
        Id = Guid.NewGuid();
        PipelineId = pipelineId;
        Key = key;
        Label = label;
        Type = type;
        Cardinality = cardinality;
        Order = order;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(
        string key,
        string label,
        PinPrimitiveType type,
        PinCardinality cardinality,
        int order
    )
    {
        Key = key;
        Label = label;
        Type = type;
        Cardinality = cardinality;
        Order = order;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
