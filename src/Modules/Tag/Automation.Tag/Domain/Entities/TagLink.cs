using System.Text.Json;

namespace Automation.Tag.Domain.Entities;

public class TagLink : BaseEntity<Guid>
{
    public Guid TagId { get; private set; }
    public TagItem Tag { get; private set; } = null!;
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public JsonDocument? Metadata { get; private set; }

    protected TagLink() { }

    public TagLink(Guid tagId, string entityType, Guid entityId, JsonDocument? metadata = null)
    {
        Id = Guid.NewGuid();
        TagId = tagId;
        EntityType = entityType;
        EntityId = entityId;
        Metadata = metadata;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
