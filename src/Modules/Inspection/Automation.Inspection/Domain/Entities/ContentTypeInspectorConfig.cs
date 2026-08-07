namespace Automation.Inspection.Domain.Entities;

public class ContentTypeInspectorConfig : BaseEntity<Guid>
{
    public Guid ContentTypeId { get; private set; }
    public string InspectorKey { get; private set; } = string.Empty;
    public string? RelevantFieldPath { get; private set; }
    public string DisplayLabel { get; private set; } = string.Empty;

    protected ContentTypeInspectorConfig() { }

    public ContentTypeInspectorConfig(Guid contentTypeId, string inspectorKey, string displayLabel, string? relevantFieldPath = null)
    {
        Id = Guid.NewGuid();
        ContentTypeId = contentTypeId;
        InspectorKey = inspectorKey;
        DisplayLabel = displayLabel;
        RelevantFieldPath = relevantFieldPath;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
