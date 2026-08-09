using Automation.SharedKernel;

namespace Automation.DynamicForms.Domain.Entities;

public class SchemaDefinition : BaseEntity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string OwnerId { get; private set; } = string.Empty;
    public string OwnerType { get; private set; } = string.Empty;

    // Navigation property
    private readonly List<SchemaVersion> _versions = new();
    public IReadOnlyCollection<SchemaVersion> Versions => _versions.AsReadOnly();

    protected SchemaDefinition() { }

    public SchemaDefinition(string name, string ownerId, string ownerType)
    {
        Id = Guid.NewGuid();
        Name = name;
        OwnerId = ownerId;
        OwnerType = ownerType;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
