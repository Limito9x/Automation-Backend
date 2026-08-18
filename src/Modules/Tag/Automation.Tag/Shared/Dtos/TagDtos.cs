namespace Automation.Tag.Shared.Dtos;

public record TagGroupDto(
    Guid Id,
    string Scope,
    string Name,
    DateTimeOffset CreatedAt
);

public record TagItemDto(
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
    string? MetadataJson
);