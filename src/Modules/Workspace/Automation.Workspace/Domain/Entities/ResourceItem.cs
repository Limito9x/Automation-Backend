namespace Automation.Workspace.Domain.Entities;

public class ResourceItem : BaseEntity<Guid>
{
    public Guid ProjectId { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public Workspace Workspace { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string? FilePath { get; private set; }
    public Guid? PlatformExtensionId { get; private set; }
    public Guid? ContentId { get; private set; }

    public ICollection<ResourceVersion> Versions { get; private set; } = new List<ResourceVersion>();

    protected ResourceItem() { }

    public ResourceItem(
        Guid projectId, 
        Guid workspaceId, 
        string name, 
        string? filePath = null, 
        Guid? platformExtensionId = null, 
        Guid? contentId = null)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        WorkspaceId = workspaceId;
        Name = name;
        FilePath = filePath;
        PlatformExtensionId = platformExtensionId;
        ContentId = contentId;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}

