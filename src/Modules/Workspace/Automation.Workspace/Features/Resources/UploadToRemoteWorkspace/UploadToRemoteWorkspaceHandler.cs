using Automation.Files.Contracts;
using Automation.Platform.Contracts;
using Automation.Workspace.Constants;
using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;

using Wolverine.Attributes;

namespace Automation.Workspace.Features.Resources.UploadToRemoteWorkspace;

[Transactional(typeof(WorkspaceDbContext))]
public class UploadToRemoteWorkspaceHandler(WorkspaceDbContext db, IPlatformApi platformApi, IAssetApi assetApi)
{
    public async Task<Result<IReadOnlyList<ResourceItemDto>>> HandleAsync(UploadToRemoteWorkspaceCommand command, CancellationToken ct)
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

        var createdResources = new List<Domain.Entities.ResourceItem>();

        foreach (var assetInput in command.Assets)
        {
            var ext = System.IO.Path.GetExtension(assetInput.Name).ToLowerInvariant();
            if (!string.IsNullOrEmpty(ext) && allowedExtensions.Count > 0 && !allowedExtensions.Contains(ext))
            {
                return Result.Fail($"Extension '{ext}' of file '{assetInput.Name}' is not allowed for this platform.");
            }

            Guid? extensionId = null;
            if (command.PlatformId.HasValue)
            {
                var extensionIdResult = await platformApi.GetExtensionIdAsync(command.PlatformId.Value, ext, ct);
                if (extensionIdResult.IsSuccess) extensionId = extensionIdResult.Value;
            }

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

