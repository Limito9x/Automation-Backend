namespace Automation.Files.Contracts;

public record AssetDto(
    Guid Id,
    string ContentType,
    long Size
);