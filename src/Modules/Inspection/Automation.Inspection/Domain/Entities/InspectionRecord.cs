using System.Text.Json;

namespace Automation.Inspection.Domain.Entities;

public class InspectionRecord : BaseEntity<Guid>
{
    public Guid ResourceVersionId { get; private set; }
    public string InspectorKey { get; private set; } = string.Empty;
    public JsonDocument ResultJson { get; private set; } = null!;
    public DateTimeOffset InspectedAt { get; private set; }

    protected InspectionRecord() { }

    public InspectionRecord(Guid resourceVersionId, string inspectorKey, JsonDocument resultJson)
    {
        Id = Guid.NewGuid();
        ResourceVersionId = resourceVersionId;
        InspectorKey = inspectorKey;
        ResultJson = resultJson;
        InspectedAt = DateTimeOffset.UtcNow;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
