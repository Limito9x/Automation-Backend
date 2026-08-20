using System.Text.Json;
using Automation.Pipeline.Constants;
using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Domain.Entities;

public class PipelineNode : BaseEntity<Guid>
{
    public Guid PipelineId { get; private set; }
    public Pipeline Pipeline { get; private set; } = null!;

    // Trong thiết kế chỉ để refId
    // Vì node có thể là tool hoặc node def (user custom) -> generic
    public string RefId { get; private set; } = string.Empty;
    public string Kind { get; private set; } = PipelineNodeKind.Custom;
    public NodePosition Position { get; private set; } = new(0, 0);
    public JsonDocument? Config { get; private set; }

    protected PipelineNode() { }


    public PipelineNode(
        Guid id,
        Guid pipelineId,
        string refId,
        string kind,
        float positionX,
        float positionY,
        JsonDocument? config = null
    )
    {
        Id = id != Guid.Empty ? id : Guid.NewGuid();
        PipelineId = pipelineId;
        RefId = refId;
        Kind = kind;
        Position = new NodePosition(positionX, positionY);
        Config = config;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(float x, float y)
    {
        Position = new NodePosition(x, y);
    }

    public void UpdateConfig(JsonDocument? config)
    {
        Config = config;
    }
}

public record NodePosition(float X, float Y);

