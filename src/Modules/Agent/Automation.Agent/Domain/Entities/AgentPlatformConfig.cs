namespace Automation.Agent.Domain.Entities;

public class AgentPlatformConfig : BaseEntity<Guid>
{
    public Guid AgentId { get; private set; }
    public Agent Agent { get; private set; } = null!;
    public Guid PlatformId { get; private set; }
    public string? Version { get; private set; }
    public string ExecutablePath { get; private set; } = string.Empty;

    protected AgentPlatformConfig() { }

    public AgentPlatformConfig(Guid agentId, Guid platformId, string executablePath, string? version = null)
    {
        Id = Guid.NewGuid();
        AgentId = agentId;
        PlatformId = platformId;
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
