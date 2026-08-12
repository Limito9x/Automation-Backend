namespace Automation.Pipeline.Domain.Entities;

public class PipelineNode : BaseEntity<Guid>
{
    public Guid PipelineId { get; private set; }
    public PipelineItem Pipeline { get; private set; } = null!;
    public Guid NodeDefinitionId { get; private set; }
    public NodeDefinition NodeDefinition { get; private set; } = null!;
    public float PositionX { get; private set; }
    public float PositionY { get; private set; }

    protected PipelineNode() { }

    public PipelineNode(Guid pipelineId, Guid nodeDefinitionId, float positionX, float positionY)
    {
        Id = Guid.NewGuid();
        PipelineId = pipelineId;
        NodeDefinitionId = nodeDefinitionId;
        PositionX = positionX;
        PositionY = positionY;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}

