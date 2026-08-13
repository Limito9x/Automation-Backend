using Automation.Platform.Contracts;
using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

using Wolverine.Attributes;

namespace Automation.Workspace.Features.Resources.SyncFromLocalWorkspace;

[Transactional(typeof(WorkspaceDbContext))]
public class SyncFromLocalWorkspaceHandler(WorkspaceDbContext db, IPlatformApi platformApi)
{
    public async Task<Result<IReadOnlyList<ResourceItemDto>>> HandleAsync(SyncFromLocalWorkspaceCommand command, CancellationToken ct)
    {
        var workspace = await db.Workspaces.FindAsync([command.WorkspaceId], ct);
        if (workspace is null)
            return Result.Fail($"Workspace with ID '{command.WorkspaceId}' was not found.");

        IReadOnlyList<string> allowedExtensions = [];
        if (command.PlatformId.HasValue)
        {
            var allowedExtensionsResult = await platformApi.GetAllowedExtensionsAsync(command.PlatformId.Value, ct);
            if (allowedExtensionsResult.IsSuccess) allowedExtensions = allowedExtensionsResult.Value;
        }

        var syncedResources = new List<Domain.Entities.ResourceItem>();

        foreach (var fileInput in command.Files)
        {
            var ext = System.IO.Path.GetExtension(fileInput.FilePath).ToLowerInvariant();
            if (!string.IsNullOrEmpty(ext) && allowedExtensions.Count > 0 && !allowedExtensions.Contains(ext))
            {
                continue; // Skip files with unsupported extensions
            }

            Guid? extensionId = null;
            if (command.PlatformId.HasValue)
            {
                var extensionIdResult = await platformApi.GetExtensionIdAsync(command.PlatformId.Value, ext, ct);
                if (extensionIdResult.IsSuccess) extensionId = extensionIdResult.Value;
            }

            var existingItem = await db.ResourceItems
                .FirstOrDefaultAsync(x => x.WorkspaceId == command.WorkspaceId && x.FilePath == fileInput.FilePath, ct);

            if (existingItem is null)
            {
                existingItem = new Domain.Entities.ResourceItem(
                    workspace.ProjectId,
                    workspace.Id,
                    fileInput.Name,
                    fileInput.FilePath,
                    extensionId,
                    null
                );

                db.ResourceItems.Add(existingItem);

                var firstVersion = new Domain.Entities.ResourceVersion(
                    existingItem.Id,
                    versionNo: 1,
                    notes: "Initial local sync",
                    fileHash: fileInput.FileHash
                );
                db.ResourceVersions.Add(firstVersion);
            }
            else
            {
                var latestVersion = await db.ResourceVersions
                    .Where(x => x.ResourceId == existingItem.Id)
                    .OrderByDescending(x => x.VersionNo)
                    .FirstOrDefaultAsync(ct);

                bool contentChanged = true;
                if (latestVersion is not null && !string.IsNullOrEmpty(fileInput.FileHash) && !string.IsNullOrEmpty(latestVersion.FileHash))
                {
                    contentChanged = !string.Equals(latestVersion.FileHash, fileInput.FileHash, StringComparison.OrdinalIgnoreCase);
                }

                if (contentChanged)
                {
                    int nextVersionNo = (latestVersion?.VersionNo ?? 0) + 1;
                    var newVersion = new Domain.Entities.ResourceVersion(
                        existingItem.Id,
                        versionNo: nextVersionNo,
                        notes: "Auto-synced local update",
                        fileHash: fileInput.FileHash
                    );
                    db.ResourceVersions.Add(newVersion);
                }
            }

            syncedResources.Add(existingItem);
        }

        await db.SaveChangesAsync(ct);

        return Result.Ok<IReadOnlyList<ResourceItemDto>>(syncedResources.Adapt<List<ResourceItemDto>>());
    }
}

