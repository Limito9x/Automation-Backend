namespace Automation.Inspection.Features.Inspectors.CreateInspectorVersion;

public record CreateInspectorVersionCommand(
    Guid InspectorId,
    string EntryPoint,
    string ScriptHash,
    bool Publish,
    Guid AssetId
);
