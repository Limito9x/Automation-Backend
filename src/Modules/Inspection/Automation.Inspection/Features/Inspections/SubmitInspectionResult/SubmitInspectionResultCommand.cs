using System.Text.Json;
using Automation.Inspection.Domain.Entities;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.Inspections.SubmitInspectionResult;

[MessageIdentity("inspection-result")]
public record SubmitInspectionResultCommand(
    Guid InspectionId,
    InspectionStatus Status,
    JsonElement? Data = null,
    long ExecutionTimeMs = 0,
    string? SummaryMessage = null
);
