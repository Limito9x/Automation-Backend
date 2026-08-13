namespace Automation.Workspace.Domain.Entities;

public class WorkspacePlatform : BaseEntity<Guid>
{
    public Guid WorkspaceId { get; private set; }
    public Workspace Workspace { get; private set; } = null!;

    public Guid PlatformId { get; private set; }

    protected WorkspacePlatform() { }

    public WorkspacePlatform(Guid workspaceId, Guid platformId)
    {
        Id = Guid.NewGuid();
        WorkspaceId = workspaceId;
        PlatformId = platformId;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
