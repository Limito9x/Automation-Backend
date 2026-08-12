using System.Text.Json;
using Automation.SharedKernel;

namespace Automation.DynamicForms.Domain.Entities;

public class SchemaVersion : BaseEntity<Guid>
{
    public Guid SchemaDefinitionId { get; private set; }
    public JsonDocument Fields { get; private set; } = null!;
    public int Version { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation property
    public SchemaDefinition SchemaDefinition { get; private set; } = null!;

    protected SchemaVersion() { }

    public SchemaVersion(Guid schemaDefinitionId, JsonDocument fields, int version, bool isActive)
    {
        Id = Guid.NewGuid();
        SchemaDefinitionId = schemaDefinitionId;
        Fields = fields;
        Version = version;
        IsActive = isActive;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

