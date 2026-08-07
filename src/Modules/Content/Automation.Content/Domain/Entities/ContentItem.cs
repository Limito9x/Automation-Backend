using System.Text.Json;

namespace Automation.Content.Domain.Entities;

public class ContentItem : BaseEntity<Guid>
{
    public Guid ContentTypeId { get; private set; }
    public ContentType ContentType { get; private set; } = null!;
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public JsonDocument Values { get; private set; } = null!;

    protected ContentItem() { }

    public ContentItem(Guid contentTypeId, Guid projectId, string name, JsonDocument values)
    {
        Id = Guid.NewGuid();
        ContentTypeId = contentTypeId;
        ProjectId = projectId;
        Name = name;
        Values = values;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string name, JsonDocument values)
    {
        Name = name;
        Values = values;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
