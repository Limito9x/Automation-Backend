using Automation.Workspace.Shared.Dtos;

namespace Automation.Workspace.Features.Workspaces.GetWorkspaceById;

public record GetWorkspaceByIdQuery(Guid Id);

public record WorkspaceDetailDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    List<WorkspaceAgentDto> WorkspaceAgents,
    DateTimeOffset CreatedAt
);