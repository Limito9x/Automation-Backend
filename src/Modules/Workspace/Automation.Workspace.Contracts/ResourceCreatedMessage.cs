namespace Automation.Workspace.Contracts;

public record ResourceVersionCreatedInfo(
    Guid ResourceVersionId,
    Guid PlatformExtensionId,
    Guid? ContentId = null
);

public record ResourcesCreatedEvent(
    Guid ProjectId,
    Guid WorkspaceId,
    Guid AgentId,
    List<ResourceVersionCreatedInfo> ResourceVersions
)
{
    public List<Guid> ResourceVersionIds => ResourceVersions.Select(r => r.ResourceVersionId).ToList();
}
