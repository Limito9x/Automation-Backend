namespace Automation.Files.Domain.Entities;

public class AssetLink : BaseEntity<Guid>
{
    public Guid AssetId { get; private set; }
    public string OwnerEntityType { get; private set; } = string.Empty;
    public string SlotKey { get; private set; } = string.Empty;
    public string OwnerEntityId { get; private set; } = string.Empty;
    public string OriginalName { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }

    public Asset Asset { get; private set; } = null!;

    protected AssetLink() { } // EF Core

    public AssetLink(Guid assetId, string ownerEntityType, string slotKey, string ownerEntityId, string originalName, int sortOrder = 0)
    {
        Id = Guid.NewGuid();
        AssetId = assetId;
        OwnerEntityType = ownerEntityType;
        SlotKey = slotKey;
        OwnerEntityId = ownerEntityId;
        OriginalName = originalName;
        SortOrder = sortOrder;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}

