namespace Automation.Inspection.Features.Inspections.TriggerInspection;

public record TriggerInspectionCommand(
    Guid ProjectId,
    IReadOnlyList<Guid> ResourceVersionIds,
    Guid? SpecificInspectorId = null
);
