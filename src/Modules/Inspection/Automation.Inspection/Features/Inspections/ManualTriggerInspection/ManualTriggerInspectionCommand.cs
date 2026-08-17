namespace Automation.Inspection.Features.Inspections.ManualTriggerInspection;

public record ManualTriggerInspectionCommand(
    Guid AgentId,
    Guid InspectorId,
    List<Guid> ResourceVersionIds
);
