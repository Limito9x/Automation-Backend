namespace Automation.Inspection.Features.Inspections.TriggerInspection;

public record InspectionRun(Guid ResourceVersionId, Guid InspectorVersionId, string ExecutorKey);

public record TriggerInspectionCommand(Guid AgentId, IReadOnlyList<InspectionRun> Runs);
