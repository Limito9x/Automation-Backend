using System.Text.Json;

namespace Automation.Inspection.Domain.Entities;

public class Inspection : BaseEntity<Guid>
{
    public Guid ResourceVersionId { get; private set; }
    public Guid InspectorVersionId { get; private set; }
    public InspectorVersion InspectorVersion { get; private set; } = null!;
    public InspectionStatus Status { get; private set; }
    public JsonDocument? Data { get; private set; }
    public long ExecutionTimeMs { get; private set; }
    public string? SummaryMessage { get; private set; }
    public DateTimeOffset? InspectedAt { get; private set; }

    protected Inspection() { }

    public Inspection(Guid resourceVersionId, Guid inspectorVersionId)
    {
        Id = Guid.NewGuid();
        ResourceVersionId = resourceVersionId;
        InspectorVersionId = inspectorVersionId;
        Status = InspectionStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkRunning()
    {
        Status = InspectionStatus.Running;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Complete(InspectionStatus status, JsonDocument data, long executionTimeMs, string? summaryMessage = null)
    {
        Status = status;
        Data = data;
        ExecutionTimeMs = executionTimeMs;
        SummaryMessage = summaryMessage;
        InspectedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Fail(string errorMessage, long executionTimeMs = 0)
    {
        Status = InspectionStatus.Failed;
        SummaryMessage = errorMessage;
        ExecutionTimeMs = executionTimeMs;
        InspectedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
