namespace Automation.Resource.Features.Resources.GetResources;

public record GetResourcesQuery(
    Guid? ProjectId = null,
    Guid? WorkspaceId = null,
    Guid? PlatformExtensionId = null,
    Guid? ContentId = null
);

