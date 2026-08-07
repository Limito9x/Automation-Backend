using System.Text.Json;

namespace Automation.Content.Domain.Entities;

public class ContentType : BaseEntity<Guid>
{
    public Guid ProjectId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public JsonDocument FieldsConfig { get; private set; } = null!;
    public JsonDocument DisplayConfig { get; private set; } = null!;

    protected ContentType() { }

    public ContentType(Guid projectId, string key, string name, JsonDocument fieldsConfig, JsonDocument displayConfig)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        Key = key;
        Name = name;
        FieldsConfig = fieldsConfig;
        DisplayConfig = displayConfig;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
