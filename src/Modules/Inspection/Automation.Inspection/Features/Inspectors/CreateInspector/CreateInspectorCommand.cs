namespace Automation.Inspection.Features.Inspectors.CreateInspector;

public record CreateInspectorCommand(
    Guid ProjectId,
    string Key,
    string Name,
    string ExecutorKey,
    string? Description = null
);
