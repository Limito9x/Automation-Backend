namespace Automation.Platform.Domain.Entities;

public class PlatformExtension : BaseEntity<Guid>
{
    public string Extension { get; private set; } = string.Empty;
    public ICollection<Platform> Platforms { get; private set; } = new List<Platform>();

    protected PlatformExtension() { }

    public PlatformExtension(string extension)
    {
        Id = Guid.NewGuid();
        Extension = extension;
    }
}
