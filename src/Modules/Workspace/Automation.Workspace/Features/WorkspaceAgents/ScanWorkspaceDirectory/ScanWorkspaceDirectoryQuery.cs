namespace Automation.Workspace.Features.WorkspaceAgents.ScanWorkspaceDirectory;

public record ScanWorkspaceDirectoryQuery(
    Guid WorkspaceId,
    Guid AgentId,
    string? RelativePath = null
);
