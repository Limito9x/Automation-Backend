namespace Automation.Inspection.Features.Inspectors.CreateInspector;

public record CreateInspectorCommand(
    Guid ProjectId,
    string Name,
    string ExecutorKey,
    string EntryPoint,
    string ScriptHash,
    Guid AssetId,
    string? Description = null
);
