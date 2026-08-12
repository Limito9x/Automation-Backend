namespace Automation.Resource.Features.Workspaces.UpdateWorkspace;

public record UpdateWorkspaceCommand(Guid Id, string Name, string? RootPath);

public class UpdateWorkspaceRequest
{
    public string Name { get; set; } = string.Empty;
    public string? RootPath { get; set; }
}

