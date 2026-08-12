using Automation.Resource.Infrastructure.Persistence;
using Automation.Resource.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Resource.Features.Workspaces.GetWorkspaceById;

public class GetWorkspaceByIdHandler(ResourceDbContext db)
{
    public async Task<Result<WorkspaceDto>> HandleAsync(GetWorkspaceByIdQuery query, CancellationToken ct)
    {
        var workspace = await db.Workspaces
            .AsNoTracking()
            .Where(x => x.Id == query.Id)
            .ProjectToType<WorkspaceDto>()
            .FirstOrDefaultAsync(ct);

        if (workspace is null)
            return Result.Fail($"Workspace with ID '{query.Id}' was not found.");

        return Result.Ok(workspace);
    }
}

