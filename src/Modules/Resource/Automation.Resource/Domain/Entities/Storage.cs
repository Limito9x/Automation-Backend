using Automation.Resource.Domain.Enums;

namespace Automation.Resource.Domain.Entities;

public class Storage : BaseEntity<Guid>
{
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public StorageKind Kind { get; private set; }
    public string? RootPath { get; private set; }

    protected Storage() { }

    public Storage(Guid projectId, string name, StorageKind kind, string? rootPath = null)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        Name = name;
        Kind = kind;
        RootPath = rootPath;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
