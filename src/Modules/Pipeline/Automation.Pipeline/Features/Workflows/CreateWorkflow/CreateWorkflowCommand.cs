namespace Automation.Pipeline.Features.Workflows.CreateWorkflow;

public record CreateWorkflowCommand(
    Guid ProjectId,
    string Name,
    string? Description
);
