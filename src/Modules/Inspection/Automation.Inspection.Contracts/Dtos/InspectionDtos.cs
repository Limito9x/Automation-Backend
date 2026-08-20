using System.Text.Json;
using Automation.Inspection.Contracts.Enums;
using Automation.Tag.Contracts.Dtos;

namespace Automation.Inspection.Contracts.Dtos;

public record InspectionDto(
    Guid Id,
    Guid ResourceVersionId,
    Guid InspectorVersionId,
    string? InspectorName,
    string? InspectorKey,
    int Version,
    string? ExecutorKey,
    InspectionStatus Status,
    JsonDocument? Data,
    long ExecutionTimeMs,
    string? SummaryMessage,
    DateTimeOffset? InspectedAt,
    DateTimeOffset CreatedAt
);

public record InspectionDetailDto(
    InspectionDto Inspection,
    Dictionary<string, IReadOnlyList<TagLinkDetailDto>> TagMap
);

public record TriggerInspectionResult(int SuccessCount, int FailedCount);
