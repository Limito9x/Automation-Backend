namespace Automation.Tag.Domain.Entities;

public class TagGroup : BaseEntity<Guid>
{
    // Thuộc trong phạm vi project
    public Guid ProjectId { get; private set; }
    public string Scope { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;

    protected TagGroup() { }

    public TagGroup(Guid projectId, string scope, string name)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        Scope = scope;
        Name = name;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string scope, string name)
    {
        Scope = scope;
        Name = name;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
