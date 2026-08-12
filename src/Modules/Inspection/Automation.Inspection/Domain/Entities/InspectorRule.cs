namespace Automation.Inspection.Domain.Entities;

public class InspectorRule : BaseEntity<Guid>
{
    public Guid ProjectId { get; private set; }
    public Guid PlatformExtensionId { get; private set; }
    public Guid? ContentTypeId { get; private set; }
    public Guid InspectorId { get; private set; }
    public Inspector Inspector { get; private set; } = null!;
    public bool Enabled { get; private set; } = true;

    protected InspectorRule() { }

    public InspectorRule(
        Guid projectId, 
        Guid platformExtensionId, 
        Guid inspectorId, 
        Guid? contentTypeId = null, 
        bool enabled = true)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        PlatformExtensionId = platformExtensionId;
        InspectorId = inspectorId;
        ContentTypeId = contentTypeId;
        Enabled = enabled;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
