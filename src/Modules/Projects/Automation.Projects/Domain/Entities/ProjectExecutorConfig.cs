using System.Text.Json;

namespace Automation.Projects.Domain.Entities;

public class ProjectExecutorConfig : BaseEntity<Guid>
{
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public Guid AgentId { get; private set; }
    public string ExecutorKey { get; private set; } = string.Empty;
    public JsonDocument? Settings { get; private set; }

    protected ProjectExecutorConfig() { }

    public ProjectExecutorConfig(
        Guid projectId,
        Guid agentId,
        string executorKey,
        JsonDocument? settings = null
    )
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        AgentId = agentId;
        ExecutorKey = executorKey;
        Settings = settings;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(JsonDocument? settings)
    {
        Settings = settings;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
