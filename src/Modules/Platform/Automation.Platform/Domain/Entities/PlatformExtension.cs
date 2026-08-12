namespace Automation.Platform.Domain.Entities;

public class PlatformExtension : BaseEntity<Guid>
{
    public Guid PlatformId { get; private set; }
    public Platform Platform { get; private set; } = null!;
    public string Extension { get; private set; } = string.Empty;

    protected PlatformExtension() { }

    public PlatformExtension(Guid platformId, string extension)
    {
        Id = Guid.NewGuid();
        PlatformId = platformId;
        Extension = extension;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
