using System.Text.Json;

namespace Automation.Content.Features.ContentItems.CreateContentItem;

public record CreateContentItemCommand(
    Guid ContentTypeId,
    Guid ProjectId,
    string Name,
    JsonDocument Values
);
