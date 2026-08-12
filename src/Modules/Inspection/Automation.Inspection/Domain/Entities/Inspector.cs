namespace Automation.Inspection.Domain.Entities;

public class Inspector : BaseEntity<Guid>
{
    public string Key { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ICollection<InspectorVersion> Versions { get; private set; } = new List<InspectorVersion>();

    protected Inspector() { }

    public Inspector(string key, string name, string? description = null)
    {
        Id = Guid.NewGuid();
        Key = key;
        Name = name;
        Description = description;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}

