using System.Text.Json;

namespace Automation.Content.Features.ContentTypes.UpdateContentType;

public record UpdateContentTypeCommand(
    Guid Id,
    string Name,
    string DisplayName,
    string? Description,
    string? Icon,
    string? Color,
    int SortOrder,
    JsonDocument FieldsConfig,
    JsonDocument DisplayConfig
);
