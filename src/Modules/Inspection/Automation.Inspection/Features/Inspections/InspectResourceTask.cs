namespace Automation.Inspection.Features.Inspections;

public record InspectResourceTask(
    Guid InspectionId,
    Guid AgentId,
    Guid ResourceVersionId,
    string ScriptUrl,
    string ScriptHash,
    string EntryPoint,
    string ExecutorKey,
    string? ResourceFilePath = null
);
