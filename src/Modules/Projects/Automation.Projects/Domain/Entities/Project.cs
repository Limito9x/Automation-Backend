namespace Automation.Projects.Domain.Entities;

public class Project : BaseEntity<Guid>
{
    public string Name { get; private set; } = string.Empty;

    protected Project() { }

    public Project(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
