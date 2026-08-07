using Automation.Projects.Domain.Enums;

namespace Automation.Projects.Domain.Entities;

public class ProjectMember : BaseEntity<Guid>
{
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public ProjectRole ProjectRole { get; private set; }

    protected ProjectMember() { }

    public ProjectMember(Guid projectId, Guid userId, ProjectRole projectRole)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        UserId = userId;
        ProjectRole = projectRole;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
