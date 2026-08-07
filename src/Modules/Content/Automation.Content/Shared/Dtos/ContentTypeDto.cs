using System.Text.Json;

namespace Automation.Content.Shared.Dtos;

public record ContentTypeDto(
    Guid Id,
    Guid ProjectId,
    string Key,
    string Name,
    string DisplayName,
    string? Description,
    string? Icon,
    string? Color,
    int SortOrder,
    JsonDocument FieldsConfig,
    JsonDocument DisplayConfig
);
