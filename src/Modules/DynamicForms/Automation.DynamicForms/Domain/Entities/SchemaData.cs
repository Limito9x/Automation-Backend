using System.Text.Json;
using Automation.SharedKernel;

namespace Automation.DynamicForms.Domain.Entities;

public class SchemaData : BaseEntity<Guid>
{
    public Guid SchemaVersionId { get; private set; }
    public JsonDocument Values { get; private set; } = null!;
    public string ClientId { get; private set; } = string.Empty;
    public string ClientType { get; private set; } = string.Empty;

    // Navigation property
    public SchemaVersion SchemaVersion { get; private set; } = null!;

    protected SchemaData() { }

    public SchemaData(Guid schemaVersionId, JsonDocument values, string clientId, string clientType)
    {
        Id = Guid.NewGuid();
        SchemaVersionId = schemaVersionId;
        Values = values;
        ClientId = clientId;
        ClientType = clientType;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateValues(JsonDocument newValues, Guid newSchemaVersionId)
    {
        Values = newValues;
        SchemaVersionId = newSchemaVersionId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
