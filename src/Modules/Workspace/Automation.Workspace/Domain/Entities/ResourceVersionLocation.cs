namespace Automation.Workspace.Domain.Entities;

public class ResourceVersionLocation : BaseEntity<Guid>
{
    public Guid ResourceVersionId { get; private set; }
    public ResourceVersion ResourceVersion { get; private set; } = null!;

    public Guid WorkspaceAgentId { get; private set; }
    public WorkspaceAgent WorkspaceAgent { get; private set; } = null!;
    public bool IsOrigin { get; private set; }
    public DateTimeOffset DiscoveredAt { get; private set; }

    protected ResourceVersionLocation() { }

    public ResourceVersionLocation(
        Guid resourceVersionId,
        Guid workspaceAgentId,
        bool isOrigin = false,
        DateTimeOffset? discoveredAt = null
    )
    {
        Id = Guid.NewGuid();
        ResourceVersionId = resourceVersionId;
        WorkspaceAgentId = workspaceAgentId;
        IsOrigin = isOrigin;
        DiscoveredAt = discoveredAt ?? DateTimeOffset.UtcNow;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    internal void SetOrigin(bool isOrigin)
    {
        IsOrigin = isOrigin;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
