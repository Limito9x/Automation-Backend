namespace Automation.Workspace.Features.Workspaces.UpdateWorkspace;

public record UpdateWorkspaceCommand(Guid Id, string Name);

public class UpdateWorkspaceRequest
{
    public string Name { get; set; } = string.Empty;
}

