namespace Automation.Inspection.Features.InspectorRules.UpdateInspectorRule;

public record UpdateInspectorRuleCommand(
    Guid Id,
    bool Enabled,
    Guid PlatformExtensionId,
    Guid? ContentTypeId = null
);
