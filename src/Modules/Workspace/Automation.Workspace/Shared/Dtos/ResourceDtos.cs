using Automation.Agent.Contracts;
using Automation.Workspace.Domain.Entities;

namespace Automation.Workspace.Shared.Dtos;

public record WorkspaceDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    int AgentCount,
    int ResourceCount,
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
    Guid AgentId,
    string RootPath,
    DateTimeOffset CreatedAt,
    AgentDto? Agent = null
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

public record DirectoryNodeDto(
    string Name,
    string Path,
    bool IsDirectory,
    long SizeBytes,
    bool HasChildren = false
);

public record BrowseDirectoryResultDto(
    string CurrentPath,
    string ParentPath,
    bool CanNavigateUp,
    IReadOnlyList<DirectoryNodeDto> Items
);

public record WorkspaceResourceDto(
    Guid Id,
    Guid ProjectId,
    Guid WorkspaceId,
    string Name,
    string? FilePath,
    Guid? PlatformExtensionId,
    Guid? ContentId,
    string? ContentName,
    string? ContentTypeName,
    string? ContentTypeColor,
    string? ContentTypeIcon,
    int VersionCount,
    DateTimeOffset CreatedAt
);

public record WorkspaceAgentResourceDto(
    Guid ResourceId,
    string ResourceName,
    string RelativePath,
    int VersionNo,
    bool IsOrigin,
    string? FileHash,
    DateTimeOffset DiscoveredAt,
    Guid? ContentId,
    string? ContentName,
    string? ContentTypeName,
    string? ContentTypeColor
);
