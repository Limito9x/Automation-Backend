using System.Text.Json;

namespace Automation.Content.Features.ContentTypes.CreateContentType;

public record CreateContentTypeCommand(
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
