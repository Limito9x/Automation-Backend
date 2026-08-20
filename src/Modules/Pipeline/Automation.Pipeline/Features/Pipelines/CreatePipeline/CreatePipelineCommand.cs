namespace Automation.Pipeline.Features.Pipelines.CreatePipeline;

public record CreatePipelineCommand(
    Guid ProjectId,
    string Name
);
