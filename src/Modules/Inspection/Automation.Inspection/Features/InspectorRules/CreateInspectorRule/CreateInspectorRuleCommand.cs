namespace Automation.Inspection.Features.InspectorRules.CreateInspectorRule;

public record CreateInspectorRuleCommand(
    Guid ProjectId,
    Guid PlatformExtensionId,
    Guid InspectorId,
    Guid? ContentTypeId = null,
    bool Enabled = true
);
