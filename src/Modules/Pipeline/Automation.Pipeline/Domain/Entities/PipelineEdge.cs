using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Domain.Entities;

public class PipelineEdge : BaseEntity<Guid>
{
    public Guid PipelineId { get; private set; }
    public Pipeline Pipeline { get; private set; } = null!;
    public Guid SourcePipelineNodeId { get; private set; }
    public PipelineNode SourcePipelineNode { get; private set; } = null!;
    public string SourcePin { get; private set; } = string.Empty;
    public Guid TargetPipelineNodeId { get; private set; }
    public PipelineNode TargetPipelineNode { get; private set; } = null!;
    public string TargetPin { get; private set; } = string.Empty;
    public EdgeKind Kind { get; private set; }

    protected PipelineEdge() { }

    public PipelineEdge(
        Guid pipelineId,
        Guid sourcePipelineNodeId,
        string sourcePin,
        Guid targetPipelineNodeId,
        string targetPin,
        EdgeKind? kind = null
    )
    {
        Id = Guid.NewGuid();
        PipelineId = pipelineId;
        SourcePipelineNodeId = sourcePipelineNodeId;
        SourcePin = sourcePin;
        TargetPipelineNodeId = targetPipelineNodeId;
        TargetPin = targetPin;
        Kind = kind ?? ((string.Equals(sourcePin, "exec_out", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(targetPin, "exec_in", StringComparison.OrdinalIgnoreCase))
            ? EdgeKind.Exec
            : EdgeKind.Data);
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
