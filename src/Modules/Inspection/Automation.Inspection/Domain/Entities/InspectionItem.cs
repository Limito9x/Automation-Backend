using System.Text.Json;

namespace Automation.Inspection.Domain.Entities;

public class InspectionItem : BaseEntity<Guid>
{
    public Guid InspectionId { get; private set; }
    public InspectionRecord Inspection { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public JsonDocument RawData { get; private set; } = null!;

    protected InspectionItem() { }

    public InspectionItem(Guid inspectionId, string name, JsonDocument rawData)
    {
        Id = Guid.NewGuid();
        InspectionId = inspectionId;
        Name = name;
        RawData = rawData;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
