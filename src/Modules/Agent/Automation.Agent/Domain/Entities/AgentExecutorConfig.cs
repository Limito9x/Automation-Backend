namespace Automation.Agent.Domain.Entities;

public class AgentExecutorConfig : BaseEntity<Guid>
{
    public Guid AgentId { get; private set; }
    public Agent Agent { get; private set; } = null!;
    public string ExecutorKey { get; private set; } = string.Empty;
    public string? Version { get; private set; }
    public string ExecutablePath { get; private set; } = string.Empty;

    protected AgentExecutorConfig() { }

    public AgentExecutorConfig(Guid agentId, string executorKey, string executablePath, string? version = null)
    {
        Id = Guid.NewGuid();
        AgentId = agentId;
        ExecutorKey = executorKey;
        ExecutablePath = executablePath;
        Version = version;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string executablePath, string? version)
    {
        ExecutablePath = executablePath;
        Version = version;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
