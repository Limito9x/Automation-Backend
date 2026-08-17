using Automation.Workspace.Contracts;
using Automation.Workspace.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Automation.Workspace.Infrastructure.Services;

public class WorkspaceApi(WorkspaceDbContext db) : IWorkspaceApi
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

        if (version == null)
            return Result.Fail("Resource version not found.");

        Guid? agentId = null;
        string? rootPath = null;

        var originLocation =
            version.Locations.FirstOrDefault(l => l.IsOrigin) ?? version.Locations[0];
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
                rootPath
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
        var resourceLocations = await db
            .ResourceVersionLocations.Where(l =>
                resourceVersionIds.Contains(l.ResourceVersionId)
                && l.WorkspaceAgent.AgentId == agentId
            )
            .Include(l => l.WorkspaceAgent)
            .Include(l => l.ResourceVersion)
                .ThenInclude(v => v.Resource)
            .ToDictionaryAsync(k => k.ResourceVersion.Id.ToString(), ct);

        if (resourceLocations.Count == 0)
            return Result.Fail("Locations not found.");

        var result = new Dictionary<string, ResourceLocationInfoDto>();
        foreach (var loc in resourceLocations)
        {
            result[loc.Key] = new ResourceLocationInfoDto(
                loc.Value.ResourceVersionId,
                loc.Value.ResourceVersion.ResourceId,
                loc.Value.ResourceVersion.Resource?.RelativePath ?? string.Empty,
                loc.Value.ResourceVersion.FileHash,
                loc.Value.WorkspaceAgent.AgentId,
                loc.Value.WorkspaceAgent.RootPath
            );
        }

        return Result.Ok(result);
    }
}
