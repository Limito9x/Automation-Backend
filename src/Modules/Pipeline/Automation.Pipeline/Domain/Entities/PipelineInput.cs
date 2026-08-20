using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Domain.Entities;

public class PipelineInput : BaseEntity<Guid>
{
    public Guid PipelineId { get; private set; }
    public Pipeline Pipeline { get; private set; } = null!;
    public string Key { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public PinPrimitiveType Type { get; private set; }
    public PinCardinality Cardinality { get; private set; } = PinCardinality.Single;
    public bool IsRequired { get; private set; } = true;
    public string? DefaultValue { get; private set; }
    public int Order { get; private set; }

    protected PipelineInput() { }

    public PipelineInput(
        Guid pipelineId,
        string key,
        string label,
        PinPrimitiveType type,
        PinCardinality cardinality = PinCardinality.Single,
        bool isRequired = true,
        string? defaultValue = null,
        int order = 0
    )
    {
        Id = Guid.NewGuid();
        PipelineId = pipelineId;
        Key = key;
        Label = label;
        Type = type;
        Cardinality = cardinality;
        IsRequired = isRequired;
        DefaultValue = defaultValue;
        Order = order;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(
        string key,
        string label,
        PinPrimitiveType type,
        PinCardinality cardinality,
        bool isRequired,
        string? defaultValue,
        int order
    )
    {
        Key = key;
        Label = label;
        Type = type;
        Cardinality = cardinality;
        IsRequired = isRequired;
        DefaultValue = defaultValue;
        Order = order;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
