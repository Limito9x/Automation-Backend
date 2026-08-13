namespace Automation.Workspace.Domain.Entities;

public class WorkspaceAgent : BaseEntity<Guid>
{
    public Guid WorkspaceId { get; private set; }
    public Workspace Workspace { get; private set; } = null!;
    public Guid AgentId { get; private set; }
    public string RootPath { get; private set; } = string.Empty;

    public ICollection<ResourceVersionLocation> Locations { get; private set; } = new List<ResourceVersionLocation>();

    protected WorkspaceAgent() { }

    public WorkspaceAgent(Guid workspaceId, Guid agentId, string rootPath)
    {
        Id = Guid.NewGuid();
        WorkspaceId = workspaceId;
        AgentId = agentId;
        RootPath = rootPath;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateRootPath(string rootPath)
    {
        RootPath = rootPath;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
