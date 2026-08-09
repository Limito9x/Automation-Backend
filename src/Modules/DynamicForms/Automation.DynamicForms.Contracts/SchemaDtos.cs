using System.Text.Json;

namespace Automation.DynamicForms.Contracts;

public class SchemaVersionDto
{
    public Guid Id { get; set; }
    public Guid SchemaDefinitionId { get; set; }
    public JsonDocument Fields { get; set; } = null!;
    public int Version { get; set; }
    public bool IsActive { get; set; }
}

public class SchemaDataDto
{
    public Guid Id { get; set; }
    public Guid SchemaVersionId { get; set; }
    public JsonDocument Values { get; set; } = null!;
    public string ClientId { get; set; } = string.Empty;
    public string ClientType { get; set; } = string.Empty;
}
