namespace Automation.Inspection.Domain.Entities;

public class Inspector : BaseEntity<Guid>
{
    public Guid ProjectId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string ExecutorKey { get; private set; } = string.Empty;
    public ICollection<InspectorVersion> Versions { get; private set; } = new List<InspectorVersion>();

    protected Inspector() { }

    public Inspector(Guid projectId, string key, string name, string executorKey, string? description = null)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        Key = key;
        Name = name;
        ExecutorKey = executorKey.ToLowerInvariant();
        Description = description;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string name, string executorKey, string? description)
    {
        Name = name;
        ExecutorKey = executorKey.ToLowerInvariant();
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
