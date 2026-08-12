using Automation.Platform.Contracts;
using Automation.Resource.Infrastructure.Persistence;
using Automation.Resource.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

using Wolverine.Attributes;

namespace Automation.Resource.Features.Resources.SyncFromLocalWorkspace;

[Transactional(typeof(ResourceDbContext))]
public class SyncFromLocalWorkspaceHandler(ResourceDbContext db, IPlatformApi platformApi)
{
    public async Task<Result<IReadOnlyList<ResourceItemDto>>> HandleAsync(SyncFromLocalWorkspaceCommand command, CancellationToken ct)
    {
        var workspace = await db.Workspaces.FindAsync([command.WorkspaceId], ct);
        if (workspace is null)
            return Result.Fail($"Workspace with ID '{command.WorkspaceId}' was not found.");

        var allowedExtensionsResult = await platformApi.GetAllowedExtensionsAsync(workspace.PlatformId, ct);
        var allowedExtensions = allowedExtensionsResult.IsSuccess ? allowedExtensionsResult.Value : [];

        var syncedResources = new List<Domain.Entities.ResourceItem>();

        foreach (var fileInput in command.Files)
        {
            var ext = System.IO.Path.GetExtension(fileInput.FilePath).ToLowerInvariant();
            if (!string.IsNullOrEmpty(ext) && allowedExtensions.Count > 0 && !allowedExtensions.Contains(ext))
            {
                continue; // Skip files with unsupported extensions
            }

            var extensionIdResult = await platformApi.GetExtensionIdAsync(workspace.PlatformId, ext, ct);
            var extensionId = extensionIdResult.IsSuccess ? extensionIdResult.Value : null;

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

