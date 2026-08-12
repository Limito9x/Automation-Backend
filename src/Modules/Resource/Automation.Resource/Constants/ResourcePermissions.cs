using Automation.SharedKernel.Abstractions.Auth;

namespace Automation.Resource.Constants;

public class ResourcePermissions
{
    public static AgentFeature Agent { get; } = new();
    public static WorkspaceFeature Workspace { get; } = new();
    public static ResourceFeature Resource { get; } = new();

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() => new()
    {
        { "Agent", Agent.All },
        { "Workspace", Workspace.All },
        { "Resource", Resource.All }
    };

    public class AgentFeature() : BaseCrudPermission("agent") { }
    public class WorkspaceFeature() : BaseCrudPermission("workspace") { }
    public class ResourceFeature() : BaseCrudPermission("resource") { }
}
