using Automation.Workspace.Contracts;
using Automation.Workspace.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

using Wolverine;

namespace Automation.Workspace.Infrastructure.Services;

public class WorkspaceApi(WorkspaceDbContext db, IMessageBus bus) : IWorkspaceApi
{
    public async Task<Result<ResourceLocationInfoDto>> GetResourceLocationAsync(
        Guid resourceVersionId,
        CancellationToken ct = default
    )
    {
        var version = await db
            .ResourceVersions.AsNoTracking()
            .Where(v => v.Id == resourceVersionId)
            .Include(v => v.Resource)
            .Include(v => v.Locations)
            .FirstOrDefaultAsync(ct);

        // Fallback: If not found by ResourceVersionId, check if it's a ResourceId and get the latest version
        if (version == null)
        {
            version = await db
                .ResourceVersions.AsNoTracking()
                .Where(v => v.ResourceId == resourceVersionId)
                .Include(v => v.Resource)
                .Include(v => v.Locations)
                .OrderByDescending(v => v.VersionNo)
                .FirstOrDefaultAsync(ct);
        }

        if (version == null)
            return Result.Fail("Resource version not found.");

        Guid? agentId = null;
        string? rootPath = null;

        var originLocation =
            version.Locations.FirstOrDefault(l => l.IsOrigin) ?? (version.Locations.Count > 0 ? version.Locations[0] : null);
        if (originLocation != null)
        {
            var wsAgent = await db
                .WorkspaceAgents.AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == originLocation.WorkspaceAgentId, ct);

            if (wsAgent != null)
            {
                agentId = wsAgent.AgentId;
                rootPath = wsAgent.RootPath;
            }
        }

        return Result.Ok(
            new ResourceLocationInfoDto(
                version.Id,
                version.ResourceId,
                version.Resource?.RelativePath ?? string.Empty,
                version.FileHash,
                agentId,
                rootPath,
                version.Resource?.ContentId
            )
        );
    }

    public async Task<
        Result<Dictionary<string, ResourceLocationInfoDto>>
    > GetResourceLocationsAsync(
        IEnumerable<Guid> resourceVersionIds,
        Guid agentId,
        CancellationToken ct = default
    )
    {
        var idsList = resourceVersionIds.ToList();
        var resourceLocations = await db
            .ResourceVersionLocations.Where(l =>
                idsList.Contains(l.ResourceVersionId)
                && l.WorkspaceAgent.AgentId == agentId
            )
            .Include(l => l.WorkspaceAgent)
            .Include(l => l.ResourceVersion)
                .ThenInclude(v => v.Resource)
            .ToDictionaryAsync(k => k.ResourceVersion.Id.ToString(), ct);

        var result = new Dictionary<string, ResourceLocationInfoDto>();
        foreach (var loc in resourceLocations)
        {
            var dto = new ResourceLocationInfoDto(
                loc.Value.ResourceVersionId,
                loc.Value.ResourceVersion.ResourceId,
                loc.Value.ResourceVersion.Resource?.RelativePath ?? string.Empty,
                loc.Value.ResourceVersion.FileHash,
                loc.Value.WorkspaceAgent.AgentId,
                loc.Value.WorkspaceAgent.RootPath,
                loc.Value.ResourceVersion.Resource?.ContentId
            );
            result[loc.Key] = dto;
            result[loc.Value.ResourceVersion.ResourceId.ToString()] = dto;
        }

        // Fallback for any missing IDs: check if they are ResourceIds
        var missingIds = idsList.Where(id => !result.ContainsKey(id.ToString())).ToList();
        if (missingIds.Count > 0)
        {
            var fallbackVersions = await db.ResourceVersions
                .AsNoTracking()
                .Where(v => missingIds.Contains(v.ResourceId))
                .Include(v => v.Resource)
                .Include(v => v.Locations)
                    .ThenInclude(l => l.WorkspaceAgent)
                .OrderByDescending(v => v.VersionNo)
                .ToListAsync(ct);

            foreach (var group in fallbackVersions.GroupBy(v => v.ResourceId))
            {
                var latest = group.First();
                var loc = latest.Locations.FirstOrDefault(l => l.WorkspaceAgent?.AgentId == agentId)
                          ?? latest.Locations.FirstOrDefault(l => l.IsOrigin)
                          ?? latest.Locations.FirstOrDefault();

                var dto = new ResourceLocationInfoDto(
                    latest.Id,
                    latest.ResourceId,
                    latest.Resource?.RelativePath ?? string.Empty,
                    latest.FileHash,
                    loc?.WorkspaceAgent?.AgentId,
                    loc?.WorkspaceAgent?.RootPath,
                    latest.Resource?.ContentId
                );
                result[group.Key.ToString()] = dto;
                result[latest.Id.ToString()] = dto;
            }
        }

        if (result.Count == 0)
            return Result.Fail("Locations not found.");

        return Result.Ok(result);
    }

    public async Task<Result<SyncLocalChangesResultDto>> SyncLocalChangesAsync(
        Guid workspaceId,
        Guid agentId,
        List<string> targetPaths,
        string? notes = null,
        CancellationToken ct = default
    )
    {
        var cmd = new Features.WorkspaceAgents.SyncLocalChanges.SyncLocalChangesCommand(
            workspaceId,
            agentId,
            notes,
            targetPaths,
            null
        );

        var result = await bus.InvokeAsync<Result<Features.WorkspaceAgents.SyncLocalChanges.SyncLocalChangesResult>>(cmd, ct);
        if (result.IsFailed)
            return Result.Fail(result.Errors);

        return Result.Ok(new SyncLocalChangesResultDto(
            result.Value.WorkspaceId,
            result.Value.AgentId,
            result.Value.AddedCount,
            result.Value.ModifiedCount,
            result.Value.LocationRemove
        ));
    }

    public async Task<Result<List<Guid>>> GetUncoveredWorkspacesAsync(
        Guid agentId,
        IEnumerable<Guid> requiredWorkspaceIds,
        CancellationToken ct = default
    )
    {
        var requiredList = requiredWorkspaceIds.Distinct().ToList();
        if (requiredList.Count == 0)
            return Result.Ok(new List<Guid>());

        var coveredWorkspaceIds = await db.WorkspaceAgents.AsNoTracking()
            .Where(w => w.AgentId == agentId && requiredList.Contains(w.WorkspaceId))
            .Select(w => w.WorkspaceId)
            .Distinct()
            .ToListAsync(ct);

        var uncovered = requiredList.Except(coveredWorkspaceIds).ToList();
        return Result.Ok(uncovered);
    }

    public async Task<Result<Dictionary<Guid, string>>> GetWorkspaceNamesAsync(
        IEnumerable<Guid> workspaceIds,
        CancellationToken ct = default
    )
    {
        var idList = workspaceIds.Distinct().ToList();
        if (idList.Count == 0)
            return Result.Ok(new Dictionary<Guid, string>());

        var dict = await db.Workspaces.AsNoTracking()
            .Where(w => idList.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id, w => w.Name, ct);

        return Result.Ok(dict);
    }

    public async Task<Result<string>> GetWorkspaceRootPathAsync(
        Guid workspaceId,
        Guid agentId,
        CancellationToken ct = default
    )
    {
        var wsAgent = await db.WorkspaceAgents.AsNoTracking()
            .FirstOrDefaultAsync(w => w.WorkspaceId == workspaceId && w.AgentId == agentId, ct);

        if (wsAgent == null)
            return Result.Fail<string>($"Workspace '{workspaceId}' is not assigned to Agent '{agentId}'.");

        return Result.Ok(wsAgent.RootPath);
    }
}
