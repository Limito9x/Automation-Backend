namespace Automation.Tag.Contracts.Dtos;

public record TagGroupDto(Guid Id, string Scope, string Name, DateTimeOffset CreatedAt);

public record TagDto(
    Guid Id,
    Guid TagGroupId,
    string Name,
    string? Color,
    DateTimeOffset CreatedAt
);

public record TagLinkDto(
    Guid Id,
    Guid TagId,
    string EntityType,
    Guid EntityId,
    string? MetadataJson,
    TagDto? Tag
);

public record TagLinkDetailDto(
    Guid TagLinkId,
    Guid TagId,
    string TagName,
    string? TagColor,
    Guid TagGroupId,
    string TagGroupScope,
    string TagGroupName,
    string? MetadataJson
);
