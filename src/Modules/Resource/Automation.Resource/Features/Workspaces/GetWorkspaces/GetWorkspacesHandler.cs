using Automation.Resource.Infrastructure.Persistence;
using Automation.Resource.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Resource.Features.Workspaces.GetWorkspaces;

internal class GetWorkspacesHandler(ResourceDbContext db)
{
    public async Task<Result<IReadOnlyList<WorkspaceDto>>> HandleAsync(GetWorkspacesQuery query, CancellationToken ct)
    {
        var dbQuery = db.Workspaces.AsNoTracking();

        if (query.ProjectId.HasValue)
            dbQuery = dbQuery.Where(x => x.ProjectId == query.ProjectId.Value);

        if (query.Kind.HasValue)
            dbQuery = dbQuery.Where(x => x.Kind == query.Kind.Value);

        if (query.AgentId.HasValue)
            dbQuery = dbQuery.Where(x => x.AgentId == query.AgentId.Value);

        var workspaces = await dbQuery
            .OrderBy(x => x.Name)
            .ProjectToType<WorkspaceDto>()
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<WorkspaceDto>>(workspaces);
    }
}
