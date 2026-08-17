namespace Automation.Workspace.Contracts;

public record ResourcesCreatedEvent(
    Guid ProjectId,
    Guid WorkspaceId,
    Guid AgentId,
    List<Guid> ResourceVersionIds
);
