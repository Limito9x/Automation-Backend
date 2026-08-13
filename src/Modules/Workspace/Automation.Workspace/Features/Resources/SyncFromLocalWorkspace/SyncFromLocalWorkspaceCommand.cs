namespace Automation.Workspace.Features.Resources.SyncFromLocalWorkspace;

public record SyncFileInput(
    string Name,
    string FilePath,
    long SizeBytes,
    string? FileHash = null
);

public record SyncFromLocalWorkspaceCommand(
    Guid WorkspaceId,
    List<SyncFileInput> Files,
    Guid? PlatformId = null
);

