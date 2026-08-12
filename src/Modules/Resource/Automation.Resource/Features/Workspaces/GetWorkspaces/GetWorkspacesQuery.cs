using Automation.Resource.Domain.Enums;

namespace Automation.Resource.Features.Workspaces.GetWorkspaces;

public record GetWorkspacesQuery(
    Guid? ProjectId = null,
    WorkspaceKind? Kind = null,
    Guid? AgentId = null
);

