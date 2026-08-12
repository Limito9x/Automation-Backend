namespace Automation.Agent.Shared.Dtos;

public record AgentDto(
    Guid Id,
    string Name,
    string MachineKey,
    bool IsActive,
    DateTimeOffset? LastSeenAt,
    DateTimeOffset CreatedAt
);

public record RegisterAgentResultDto(
    Guid Id,
    string Name,
    string MachineKey,
    string RegistrationToken
);

public record AgentPlatformConfigDto(
    Guid Id,
    Guid AgentId,
    Guid PlatformId,
    string ExecutablePath,
    string? Version,
    DateTimeOffset CreatedAt
);
