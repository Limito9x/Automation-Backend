namespace Automation.Workspace.Constants;

public static class WorkspaceRoutes
{
    public const string NestedWorkspaces = "/projects/{projectId:guid}/workspaces";
    public const string Workspaces = "/workspaces";
    public const string Workspace = "/workspaces/{id:guid}";
    public const string AttachAgent = "/workspaces/{workspaceId:guid}/agents";
    public const string ScanDirectory = "/workspaces/{workspaceId:guid}/agents/{agentId:guid}/scan-dir";
}
