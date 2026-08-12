namespace Automation.Pipeline.Domain.Entities;

public class PipelineEdge : BaseEntity<Guid>
{
    public Guid PipelineId { get; private set; }
    public PipelineItem Pipeline { get; private set; } = null!;
    public Guid SourcePipelineNodeId { get; private set; }
    public PipelineNode SourcePipelineNode { get; private set; } = null!;
    public string SourcePin { get; private set; } = string.Empty;
    public Guid TargetPipelineNodeId { get; private set; }
    public PipelineNode TargetPipelineNode { get; private set; } = null!;
    public string TargetPin { get; private set; } = string.Empty;

    protected PipelineEdge() { }

    public PipelineEdge(Guid pipelineId, Guid sourcePipelineNodeId, string sourcePin, Guid targetPipelineNodeId, string targetPin)
    {
        Id = Guid.NewGuid();
        PipelineId = pipelineId;
        SourcePipelineNodeId = sourcePipelineNodeId;
        SourcePin = sourcePin;
        TargetPipelineNodeId = targetPipelineNodeId;
        TargetPin = targetPin;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}

