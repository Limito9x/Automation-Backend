namespace Automation.Inspection.Features.Inspectors.UpdateInspector;

public record UpdateInspectorCommand(
    Guid Id,
    string Name,
    string ExecutorKey,
    string? Description = null
);
