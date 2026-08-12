using Automation.Resource.Domain.Enums;

namespace Automation.Resource.Features.Workspaces.CreateWorkspace;

public record CreateWorkspaceCommand(
    Guid ProjectId,
    Guid PlatformId,
    string Name,
    WorkspaceKind Kind,
    string? RootPath = null,
    Guid? AgentId = null
);
