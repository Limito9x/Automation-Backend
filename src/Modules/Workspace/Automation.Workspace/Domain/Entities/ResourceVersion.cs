namespace Automation.Workspace.Domain.Entities;

public class ResourceVersion : BaseEntity<Guid>
{
    public Guid ResourceId { get; private set; }
    public ResourceItem Resource { get; private set; } = null!;
    public int VersionNo { get; private set; }
    public string? Notes { get; private set; }
    public string FileHash { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public System.Text.Json.JsonDocument? Metadata { get; private set; }
    private readonly List<ResourceVersionLocation> _locations = new();
    public IReadOnlyList<ResourceVersionLocation> Locations => _locations.AsReadOnly();

    private ResourceVersion() { }

    internal ResourceVersion(
        Guid resourceId,
        int versionNo,
        long sizeBytes,
        string fileHash,
        string? notes = null
    )
    {
        Id = Guid.NewGuid();
        ResourceId = resourceId;
        VersionNo = versionNo;
        Notes = notes;
        FileHash = fileHash;
        SizeBytes = sizeBytes;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    internal ResourceVersionLocation AddLocation(
        Guid workspaceAgentId,
        bool isOrigin = false,
        DateTimeOffset? discoveredAt = null
    )
    {
        var existing = _locations.FirstOrDefault(x => x.WorkspaceAgentId == workspaceAgentId);
        if (existing != null)
        {
            if (isOrigin)
                MarkLocationAsOrigin(workspaceAgentId);
            return existing;
        }

        // Chưa có location => version đầu tiên
        if (_locations.Count == 0)
        {
            isOrigin = true;
        }

        if (isOrigin)
        {
            foreach (var loc in _locations)
            {
                loc.SetOrigin(false);
            }
        }

        var location = new ResourceVersionLocation(Id, workspaceAgentId, isOrigin, discoveredAt);
        _locations.Add(location);
        return location;
    }

    public void MarkLocationAsOrigin(Guid workspaceAgentId)
    {
        var location =
            _locations.FirstOrDefault(x => x.WorkspaceAgentId == workspaceAgentId)
            ?? throw new InvalidOperationException("Location not found");

        foreach (var loc in _locations)
        {
            loc.SetOrigin(false);
        }

        location.SetOrigin(true);
    }

    public void SetMetadata(System.Text.Json.JsonDocument? metadata)
    {
        Metadata = metadata;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
