namespace Automation.Pipeline.Features.Workflows.UpdateWorkflow;

public record UpdateWorkflowCommand(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive
);
