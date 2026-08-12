namespace Automation.Agent.Features.Agents.ConfigureAgentPlatform;

public record ConfigureAgentPlatformCommand(
    Guid AgentId,
    Guid PlatformId,
    string ExecutablePath,
    string? Version
);
