using Automation.Workspace.Contracts;
using Automation.Workspace.Domain.Entities;
using Automation.Workspace.Features.WorkspaceAgents.CompareWorkspaceResources;
using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Workspace.Features.WorkspaceAgents.SyncLocalChanges;

[Transactional(typeof(WorkspaceDbContext))]
public class SyncLocalChangesHandler(WorkspaceDbContext dbContext, IMessageBus bus)
{
    public async Task<Result<SyncLocalChangesResult>> HandleAsync(
        SyncLocalChangesCommand cmd,
        CancellationToken ct
    )
    {
        var compareCmd = new CompareWorkspaceResourcesCommand(cmd.WorkspaceId, cmd.AgentId);

        var diffResult = await bus.InvokeAsync<Result<DiffResult>>(compareCmd);
        if (diffResult.IsFailed)
            return Result.Fail(diffResult.Errors);

        var diff = diffResult.Value;

        var workspaceAgentId = diff.WorkspaceAgentId;

        // Do sử dụng Guid làm Id của entity -> khi khởi tạo đã có ngay id
        // Có thể đẩy vào mảng sau đó raise event
        var createdVersions = new List<ResourceVersionCreatedInfo>();

        var toAdd = diff
            .Added.Where(x => cmd.TargetPaths.Contains(x.RelativePath))
            .Select(x =>
                ResourceItem.Create(
                    cmd.WorkspaceId,
                    workspaceAgentId,
                    x.PlatformExtensionId,
                    cmd.NewResourceNames?.GetValueOrDefault(x.RelativePath) ?? x.Name,
                    x.RelativePath,
                    x.LocalHash ?? "",
                    x.LocalFileSize ?? 0
                )
            )
            .ToList();

        if (toAdd.Count > 0)
        {
            createdVersions.AddRange(
                toAdd.Select(x => new ResourceVersionCreatedInfo(
                    x.LatestVersion!.Id,
                    x.PlatformExtensionId,
                    x.ContentId
                ))
            );
            dbContext.ResourceItems.AddRange(toAdd);
        }

        var pathsToFetch = diff
            .Modified.Concat(diff.Deleted)
            .Select(x => x.RelativePath)
            .Where(path => cmd.TargetPaths.Contains(path))
            .ToList();

        var existingItems = await dbContext
            .ResourceItems.Include(r => r.Versions)
                .ThenInclude(v => v.Locations)
            .Where(x => x.WorkspaceId == cmd.WorkspaceId && pathsToFetch.Contains(x.RelativePath))
            .ToDictionaryAsync(x => x.RelativePath, y => y, ct);

        var modCount = 0;
        foreach (var mod in diff.Modified.Where(x => cmd.TargetPaths.Contains(x.RelativePath)))
        {
            if (existingItems.TryGetValue(mod.RelativePath, out var item))
            {
                var newVersion = item.AddNewVersion(
                    workspaceAgentId,
                    mod.LocalHash ?? "",
                    mod.LocalFileSize ?? 0,
                    cmd.Notes
                );
                dbContext.ResourceVersions.Add(newVersion);
                modCount++;
                createdVersions.Add(new ResourceVersionCreatedInfo(
                    newVersion.Id,
                    item.PlatformExtensionId,
                    item.ContentId
                ));
            }
        }

        var locations = existingItems
            .Where(x => diff.Deleted.Any(y => y.RelativePath == x.Key))
            .SelectMany(x => x.Value.Versions)
            .SelectMany(v => v.Locations)
            .Where(loc => loc.WorkspaceAgentId == workspaceAgentId)
            .ToList();

        var locationRemove = 0;
        if (locations.Count > 0)
        {
            dbContext.ResourceVersionLocations.RemoveRange(locations);
            locationRemove = locations.Count;
        }

        await dbContext.SaveChangesAsync(ct);

        // Sau khi thành công, lấy ProjectId và raise event
        if (createdVersions.Count > 0)
        {
            var projectId = await dbContext.Workspaces
                .AsNoTracking()
                .Where(w => w.Id == cmd.WorkspaceId)
                .Select(w => w.ProjectId)
                .FirstOrDefaultAsync(ct);

            await bus.PublishAsync(
                new ResourcesCreatedEvent(
                    projectId,
                    cmd.WorkspaceId,
                    cmd.AgentId,
                    createdVersions
                )
            );
        }

        return Result.Ok(
            new SyncLocalChangesResult(
                cmd.WorkspaceId,
                workspaceAgentId,
                toAdd.Count,
                modCount,
                locationRemove
            )
        );
    }
}
