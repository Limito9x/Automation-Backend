using System.Text.Json;

namespace Automation.Content.Domain.Entities;

public class ContentType : BaseEntity<Guid>
{
    public Guid ProjectId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Icon { get; private set; }
    public string? Color { get; private set; }
    public int SortOrder { get; private set; }
    public JsonDocument DisplayConfig { get; private set; } = null!;

    protected ContentType() { }

    public ContentType(Guid projectId, string key, string name, string displayName, string? description, string? icon, string? color, int sortOrder, JsonDocument displayConfig)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        Key = key;
        Name = name;
        DisplayName = displayName;
        Description = description;
        Icon = icon;
        Color = color;
        SortOrder = sortOrder;
        DisplayConfig = displayConfig;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string name, string displayName, string? description, string? icon, string? color, int sortOrder, JsonDocument displayConfig)
    {
        Name = name;
        DisplayName = displayName;
        Description = description;
        Icon = icon;
        Color = color;
        SortOrder = sortOrder;
        DisplayConfig = displayConfig;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
