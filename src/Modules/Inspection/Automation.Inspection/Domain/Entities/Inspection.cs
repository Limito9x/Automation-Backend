using System.Text.Json;

namespace Automation.Inspection.Domain.Entities;

public class Inspection : BaseEntity<Guid>
{
    public Guid ResourceId { get; private set; }
    public Guid InspectorVersionId { get; private set; }
    public InspectorVersion InspectorVersion { get; private set; } = null!;
    public int Version { get; private set; }
    public JsonDocument Data { get; private set; } = null!;
    public DateTimeOffset InspectedAt { get; private set; }

    protected Inspection() { }

    public Inspection(Guid resourceId, Guid inspectorVersionId, int version, JsonDocument data)
    {
        Id = Guid.NewGuid();
        ResourceId = resourceId;
        InspectorVersionId = inspectorVersionId;
        Version = version;
        Data = data;
        InspectedAt = DateTimeOffset.UtcNow;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}

