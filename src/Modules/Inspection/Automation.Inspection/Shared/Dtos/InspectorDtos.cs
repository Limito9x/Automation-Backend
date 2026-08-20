using System.Text.Json;
using Automation.Inspection.Domain.Entities;

namespace Automation.Inspection.Shared.Dtos;

public record InspectorDto(
    Guid Id,
    Guid ProjectId,
    string Key,
    string Name,
    string ExecutorKey,
    string? Description,
    DateTimeOffset CreatedAt,
    IReadOnlyList<InspectorVersionDto>? Versions = null
);

public record InspectorVersionDto(
    Guid Id,
    Guid InspectorId,
    string Version,
    string EntryPoint,
    string ScriptHash,
    bool IsPublished,
    DateTimeOffset CreatedAt
);

public record InspectorRuleDto(
    Guid Id,
    Guid ProjectId,
    Guid PlatformExtensionId,
    Guid? ContentTypeId,
    Guid InspectorId,
    string? InspectorName,
    string? InspectorKey,
    string? ExecutorKey,
    bool Enabled,
    DateTimeOffset CreatedAt
);

