namespace Automation.Platform.Domain.Entities;

public class Platform : BaseEntity<Guid>
{
    public string Key { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;

    protected Platform() { } // EF Core

    public Platform(string key, string name)
    {
        Id = Guid.NewGuid();
        Key = key;
        Name = name;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
