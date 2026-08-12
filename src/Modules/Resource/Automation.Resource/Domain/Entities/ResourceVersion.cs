namespace Automation.Resource.Domain.Entities;

public class ResourceVersion : BaseEntity<Guid>
{
    public Guid ResourceId { get; private set; }
    public ResourceItem Resource { get; private set; } = null!;
    public int VersionNo { get; private set; }
    public string? Notes { get; private set; }
    public string? FileHash { get; private set; }

    protected ResourceVersion() { }

    public ResourceVersion(Guid resourceId, int versionNo, string? notes = null, string? fileHash = null)
    {
        Id = Guid.NewGuid();
        ResourceId = resourceId;
        VersionNo = versionNo;
        Notes = notes;
        FileHash = fileHash;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}

