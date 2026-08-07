namespace Automation.Resource.Domain.Entities;

public class ResourceVersion : BaseEntity<Guid>
{
    public Guid ResourceId { get; private set; }
    public ResourceItem Resource { get; private set; } = null!;
    public int VersionNo { get; private set; }
    public Guid AssetId { get; private set; }

    protected ResourceVersion() { }

    public ResourceVersion(Guid resourceId, int versionNo, Guid assetId)
    {
        Id = Guid.NewGuid();
        ResourceId = resourceId;
        VersionNo = versionNo;
        AssetId = assetId;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
