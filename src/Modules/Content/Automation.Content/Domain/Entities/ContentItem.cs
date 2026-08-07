using System.Text.Json;

namespace Automation.Content.Domain.Entities;

public class ContentItem : BaseEntity<Guid>
{
    public Guid ContentTypeId { get; private set; }
    public ContentType ContentType { get; private set; } = null!;
    public Guid ProjectId { get; private set; }
    public JsonDocument Values { get; private set; } = null!;

    protected ContentItem() { }

    public ContentItem(Guid contentTypeId, Guid projectId, JsonDocument values)
    {
        Id = Guid.NewGuid();
        ContentTypeId = contentTypeId;
        ProjectId = projectId;
        Values = values;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
