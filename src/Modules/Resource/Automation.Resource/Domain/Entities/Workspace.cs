using Automation.Resource.Domain.Enums;

namespace Automation.Resource.Domain.Entities;

public class Workspace : BaseEntity<Guid>
{
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public WorkspaceKind Kind { get; private set; }
    public string? RootPath { get; private set; }
    public ICollection<ResourceItem> Resources { get; private set; } = new List<ResourceItem>();

    protected Workspace() { }

    public Workspace(Guid projectId, string name, WorkspaceKind kind, string? rootPath = null)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        Name = name;
        Kind = kind;
        RootPath = rootPath;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
