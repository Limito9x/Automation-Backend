using Automation.Files.Contracts;
using Automation.Platform.Contracts;
using Automation.Resource.Constants;
using Automation.Resource.Infrastructure.Persistence;
using Automation.Resource.Shared.Dtos;

using Wolverine.Attributes;

namespace Automation.Resource.Features.Resources.UploadToRemoteWorkspace;

[Transactional(typeof(ResourceDbContext))]
public class UploadToRemoteWorkspaceHandler(ResourceDbContext db, IPlatformApi platformApi, IAssetApi assetApi)
{
    public async Task<Result<IReadOnlyList<ResourceItemDto>>> HandleAsync(UploadToRemoteWorkspaceCommand command, CancellationToken ct)
    {
        var workspace = await db.Workspaces.FindAsync([command.WorkspaceId], ct);
        if (workspace is null)
            return Result.Fail($"Workspace with ID '{command.WorkspaceId}' was not found.");

        var allowedExtensionsResult = await platformApi.GetAllowedExtensionsAsync(workspace.PlatformId, ct);
        var allowedExtensions = allowedExtensionsResult.IsSuccess ? allowedExtensionsResult.Value : [];

        var createdResources = new List<Domain.Entities.ResourceItem>();

        foreach (var assetInput in command.Assets)
        {
            var ext = System.IO.Path.GetExtension(assetInput.Name).ToLowerInvariant();
            if (!string.IsNullOrEmpty(ext) && allowedExtensions.Count > 0 && !allowedExtensions.Contains(ext))
            {
                return Result.Fail($"Extension '{ext}' of file '{assetInput.Name}' is not allowed for this workspace's platform.");
            }

            var extensionIdResult = await platformApi.GetExtensionIdAsync(workspace.PlatformId, ext, ct);
            var extensionId = extensionIdResult.IsSuccess ? extensionIdResult.Value : null;

            var resourceItem = new Domain.Entities.ResourceItem(
                workspace.ProjectId,
                workspace.Id,
                assetInput.Name,
                filePath: null,
                extensionId,
                command.ContentId
            );

            db.ResourceItems.Add(resourceItem);

            var version = new Domain.Entities.ResourceVersion(
                resourceItem.Id,
                versionNo: 1,
                notes: "Remote upload"
            );

            db.ResourceVersions.Add(version);
            await db.SaveChangesAsync(ct);

            // Link asset in Files module
            var linkResult = await assetApi.VerifyAndLinkAsync(
                assetId: assetInput.AssetId,
                ownerEntityType: "ResourceVersion",
                slotKey: ResourceAssetSlots.ResourceVersion,
                ownerEntityId: version.Id.ToString(),
                originalName: assetInput.Name,
                sortOrder: 0,
                ct: ct
            );

            if (linkResult.IsFailed)
            {
                return Result.Fail($"Failed to link asset '{assetInput.AssetId}': {linkResult.Errors.FirstOrDefault()?.Message}");
            }

            createdResources.Add(resourceItem);
        }

        return Result.Ok<IReadOnlyList<ResourceItemDto>>(createdResources.Adapt<List<ResourceItemDto>>());
    }
}

