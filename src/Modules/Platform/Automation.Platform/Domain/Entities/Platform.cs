namespace Automation.Platform.Domain.Entities;

public class Platform : BaseEntity<Guid>
{
    public string Key { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public ICollection<PlatformExtension> Extensions { get; private set; } = new List<PlatformExtension>();

    protected Platform() { }

    public Platform(string key, string name)
    {
        Id = Guid.NewGuid();
        Key = key;
        Name = name;
    }

    public void Update(string name)
    {
        Name = name;
    }

    public void SetExtensions(IEnumerable<PlatformExtension> extensions)
    {
        Extensions.Clear();
        foreach (var ext in extensions)
        {
            Extensions.Add(ext);
        }
    }
}
