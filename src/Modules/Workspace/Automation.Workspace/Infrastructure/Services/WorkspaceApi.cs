using Automation.Workspace.Contracts;
using Automation.Workspace.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Automation.Workspace.Infrastructure.Services;

public class WorkspaceApi(WorkspaceDbContext db) : IWorkspaceApi
{
    public async Task<Result<ResourceLocationInfoDto>> GetResourceLocationAsync(Guid resourceVersionId, CancellationToken ct = default)
    {
        var version = await db.ResourceVersions
            .AsNoTracking()
            .Where(v => v.Id == resourceVersionId)
            .Include(v => v.Resource)
            .Include(v => v.Locations)
            .FirstOrDefaultAsync(ct);

        if (version == null)
            return Result.Fail("Resource version not found.");

        Guid? agentId = null;
        string? rootPath = null;

        var originLocation = version.Locations.FirstOrDefault(l => l.IsOrigin) ?? version.Locations.FirstOrDefault();
        if (originLocation != null)
        {
            var wsAgent = await db.WorkspaceAgents
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == originLocation.WorkspaceAgentId, ct);

            if (wsAgent != null)
            {
                agentId = wsAgent.AgentId;
                rootPath = wsAgent.RootPath;
            }
        }

        return Result.Ok(new ResourceLocationInfoDto(
            version.Id,
            version.ResourceId,
            version.Resource?.RelativePath ?? string.Empty,
            version.FileHash,
            agentId,
            rootPath
        ));
    }
}
