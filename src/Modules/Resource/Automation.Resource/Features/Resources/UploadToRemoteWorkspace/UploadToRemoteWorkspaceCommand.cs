namespace Automation.Resource.Features.Resources.UploadToRemoteWorkspace;

public record RemoteAssetInput(
    Guid AssetId,
    string Name
);

public record UploadToRemoteWorkspaceCommand(
    Guid WorkspaceId,
    List<RemoteAssetInput> Assets,
    Guid? ContentId = null
);
