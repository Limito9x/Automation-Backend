using System.Text.Json;
using Automation.Tag.Contracts.Dtos;

namespace Automation.Workspace.Contracts.Dtos;

public record ResourceMetadataDetailDto(
    Guid ResourceVersionId,
    JsonDocument? Metadata,
    Dictionary<string, IReadOnlyList<TagLinkDetailDto>> TagMap
);

public record UpdatedTagLink(
    Guid TagId,
    string JsonPath
);
