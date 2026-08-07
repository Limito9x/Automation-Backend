using System.Text.Json;

namespace Automation.Content.Features.ContentItems.UpdateContentItem;

public record UpdateContentItemCommand(
    Guid Id,
    string Name,
    JsonDocument Values
);
