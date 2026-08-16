namespace Automation.Inspection.Domain.Entities;

public class InspectorVersion : BaseEntity<Guid>
{
    public Guid InspectorId { get; private set; }
    public Inspector Inspector { get; private set; } = null!;
    public string Version { get; private set; } = string.Empty;
    public string EntryPoint { get; private set; } = string.Empty;
    public string ScriptHash { get; private set; } = string.Empty;
    public bool IsPublished { get; private set; }

    protected InspectorVersion() { }

    public InspectorVersion(
        Guid inspectorId,
        string version,
        string entryPoint,
        string scriptHash,
        bool isPublished = false)
    {
        Id = Guid.NewGuid();
        InspectorId = inspectorId;
        Version = version;
        EntryPoint = entryPoint;
        ScriptHash = scriptHash;
        IsPublished = isPublished;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void SetPublished(bool isPublished)
    {
        IsPublished = isPublished;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}