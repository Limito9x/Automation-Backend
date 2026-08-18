using System.Text.Json;

namespace Automation.Tag.Features.TagLinks.CreateTagLink;

public record CreateTagLinkCommand(
    Guid TagId,
    string EntityType,
    Guid EntityId,
    JsonDocument? Metadata = null
);
