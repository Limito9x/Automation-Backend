namespace Automation.Agent.Contracts;

public record AgentExecutorConfigDto(
    Guid Id,
    Guid AgentId,
    string ExecutorKey,
    string ExecutablePath,
    string? Version,
    DateTimeOffset CreatedAt
);
