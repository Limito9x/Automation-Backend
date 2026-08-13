using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Workspace.Features.Workspaces.GetWorkspaces;

public class GetWorkspacesHandler(WorkspaceDbContext db)
{
    public async Task<Result<IReadOnlyList<WorkspaceDto>>> HandleAsync(GetWorkspacesQuery query, CancellationToken ct)
    {
        var dbQuery = db.Workspaces.AsNoTracking();

        if (query.ProjectId.HasValue)
            dbQuery = dbQuery.Where(x => x.ProjectId == query.ProjectId.Value);

        var workspaces = await dbQuery
            .OrderBy(x => x.Name)
            .ProjectToType<WorkspaceDto>()
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<WorkspaceDto>>(workspaces);
    }
}

