namespace Automation.Workspace.Features.WorkspaceAgents.AttachAgentToWorkspace;

public record AttachAgentToWorkspaceCommand(
    Guid WorkspaceId,
    Guid AgentId,
    string RootPath
);

public class AttachAgentToWorkspaceRequest
{
    public Guid AgentId { get; set; }
    public string RootPath { get; set; } = string.Empty;
}
