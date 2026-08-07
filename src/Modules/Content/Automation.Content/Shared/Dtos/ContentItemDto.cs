using System.Text.Json;

namespace Automation.Content.Shared.Dtos;

public record ContentItemDto(
    Guid Id,
    Guid ContentTypeId,
    Guid ProjectId,
    string Name,
    JsonDocument Values
);
