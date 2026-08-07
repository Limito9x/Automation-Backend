namespace Automation.Resource.Domain.Entities;

public class ResourceItem : BaseEntity<Guid>
{
    public Guid ProjectId { get; private set; }
    public Guid StorageId { get; private set; }
    public Storage Storage { get; private set; } = null!;
    public Guid AssetId { get; private set; }

    protected ResourceItem() { }

    public ResourceItem(Guid projectId, Guid storageId, Guid assetId)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        StorageId = storageId;
        AssetId = assetId;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
