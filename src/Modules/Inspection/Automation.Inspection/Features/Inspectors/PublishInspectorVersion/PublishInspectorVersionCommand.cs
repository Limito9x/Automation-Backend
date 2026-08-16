namespace Automation.Inspection.Features.Inspectors.PublishInspectorVersion;

public record PublishInspectorVersionCommand(
    Guid VersionId,
    bool IsPublished = true
);
