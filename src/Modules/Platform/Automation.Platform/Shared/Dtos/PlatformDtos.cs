namespace Automation.Platform.Shared.Dtos;

public record PlatformDto(
    Guid Id,
    string Key,
    string Name,
    List<string> Extensions,
    DateTimeOffset CreatedAt
);

public record PlatformExtensionDto(
    Guid Id,
    string Extension,
    DateTimeOffset CreatedAt
);
