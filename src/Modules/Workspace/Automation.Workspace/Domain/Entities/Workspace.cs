namespace Automation.Workspace.Domain.Entities;

public class Workspace : BaseEntity<Guid>
{
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ICollection<ResourceItem> Resources { get; private set; } = new List<ResourceItem>();
    public ICollection<WorkspaceAgent> WorkspaceAgents { get; private set; } = new List<WorkspaceAgent>();
    public ICollection<WorkspacePlatform> WorkspacePlatforms { get; private set; } = new List<WorkspacePlatform>();

    protected Workspace() { }

    public Workspace(Guid projectId, string name)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        Name = name;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string name)
    {
        Name = name;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddPlatform(Guid platformId)
    {
        if (!WorkspacePlatforms.Any(x => x.PlatformId == platformId))
        {
            WorkspacePlatforms.Add(new WorkspacePlatform(Id, platformId));
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void RemovePlatform(Guid platformId)
    {
        var wp = WorkspacePlatforms.FirstOrDefault(x => x.PlatformId == platformId);
        if (wp is not null)
        {
            WorkspacePlatforms.Remove(wp);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}

