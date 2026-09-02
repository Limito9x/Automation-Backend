using System.Text.Json;

namespace Automation.Tag.Domain.Entities;

public class TagLink : BaseEntity<Guid>
{
    // Tham chiếu project lọc nhanh hơn
    public Guid ProjectId { get; private set; }
    public Guid TagId { get; private set; }
    public TagItem Tag { get; private set; } = null!;
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public JsonDocument? Metadata { get; private set; }

    protected TagLink() { }

    public TagLink(
        Guid projectId,
        Guid tagId,
        string entityType,
        Guid entityId,
        JsonDocument? metadata = null
    )
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        TagId = tagId;
        EntityType = entityType;
        EntityId = entityId;
        Metadata = metadata;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateMetadata(JsonDocument? metadata)
    {
        Metadata = metadata;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
