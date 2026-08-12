using Automation.Resource.Domain.Enums;

namespace Automation.Resource.Domain.Entities;

public class Workspace : BaseEntity<Guid>
{
    public Guid ProjectId { get; private set; }
    public Guid PlatformId { get; private set; }
    public Guid? AgentId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public WorkspaceKind Kind { get; private set; }
    public string? RootPath { get; private set; }
    public ICollection<ResourceItem> Resources { get; private set; } = new List<ResourceItem>();

    protected Workspace() { }

    public Workspace(Guid projectId, Guid platformId, string name, WorkspaceKind kind, string? rootPath = null, Guid? agentId = null)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        PlatformId = platformId;
        Name = name;
        Kind = kind;
        RootPath = rootPath;
        AgentId = agentId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string name, string? rootPath)
    {
        Name = name;
        RootPath = rootPath;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
