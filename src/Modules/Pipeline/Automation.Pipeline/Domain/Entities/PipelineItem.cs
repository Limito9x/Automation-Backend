namespace Automation.Pipeline.Domain.Entities;

public class PipelineItem : BaseEntity<Guid>
{
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    protected PipelineItem() { }

    public PipelineItem(Guid projectId, string name)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        Name = name;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
