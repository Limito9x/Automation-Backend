namespace Automation.Workspace.Shared.Dtos;

public record WorkspaceDto(
    Guid Id,
    Guid ProjectId,
    string Name,
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

public record WorkspaceAgentDto(
    Guid Id,
    Guid WorkspaceId,
    Guid AgentId,
    string RootPath,
    DateTimeOffset CreatedAt
);

public record ResourceVersionLocationDto(
    Guid Id,
    Guid ResourceVersionId,
    Guid WorkspaceAgentId,
    string RelativePath,
    bool IsOrigin,
    DateTimeOffset DiscoveredAt,
    DateTimeOffset CreatedAt
);

