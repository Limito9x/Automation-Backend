namespace Automation.Inspection.Features.Inspectors.CreateInspectorVersion;

public record CreateInspectorVersionCommand(
    Guid InspectorId,
    string Version,
    string EntryPoint,
    string ScriptHash,
    Guid AssetId,
    string? OriginalFileName = null,
    bool IsPublished = false
);
