namespace Automation.Workspace.Domain.Entities;

public class ResourceVersionLocation : BaseEntity<Guid>
{
    public Guid ResourceVersionId { get; private set; }
    public ResourceVersion ResourceVersion { get; private set; } = null!;

    public Guid WorkspaceAgentId { get; private set; }
    public WorkspaceAgent WorkspaceAgent { get; private set; } = null!;

    public string RelativePath { get; private set; } = string.Empty;
    public bool IsOrigin { get; private set; }
    public DateTimeOffset DiscoveredAt { get; private set; }

    protected ResourceVersionLocation() { }

    public ResourceVersionLocation(
        Guid resourceVersionId,
        Guid workspaceAgentId,
        string relativePath,
        bool isOrigin = false,
        DateTimeOffset? discoveredAt = null)
    {
        Id = Guid.NewGuid();
        ResourceVersionId = resourceVersionId;
        WorkspaceAgentId = workspaceAgentId;
        RelativePath = relativePath;
        IsOrigin = isOrigin;
        DiscoveredAt = discoveredAt ?? DateTimeOffset.UtcNow;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsOrigin(bool isOrigin)
    {
        IsOrigin = isOrigin;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateRelativePath(string relativePath)
    {
        RelativePath = relativePath;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateVersion(Guid newVersionId)
    {
        ResourceVersionId = newVersionId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
