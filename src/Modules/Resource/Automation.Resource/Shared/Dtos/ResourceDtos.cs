using Automation.Resource.Domain.Enums;

namespace Automation.Resource.Shared.Dtos;

public record WorkspaceDto(
    Guid Id,
    Guid ProjectId,
    Guid PlatformId,
    Guid? AgentId,
    string Name,
    WorkspaceKind Kind,
    string? RootPath,
    DateTimeOffset CreatedAt
);

public record ResourceItemDto(
    Guid Id,
    Guid ProjectId,
    Guid WorkspaceId,
    string Name,
    string? FilePath,
    Guid? PlatformExtensionId,
    Guid? ContentId,
    DateTimeOffset CreatedAt
);

public record ResourceVersionDto(
    Guid Id,
    Guid ResourceId,
    int VersionNo,
    string? Notes,
    string? FileHash,
    DateTimeOffset CreatedAt
);
