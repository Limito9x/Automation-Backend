using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Workspace.Features.Resources.GetResources;

[NonTransactional]
public class GetResourcesHandler(WorkspaceDbContext db)
{
    public async Task<Result<IReadOnlyList<ResourceItemDto>>> HandleAsync(GetResourcesQuery query, CancellationToken ct)
    {
        var dbQuery = db.ResourceItems.AsNoTracking();

        if (query.ProjectId.HasValue)
            dbQuery = dbQuery.Where(x => x.ProjectId == query.ProjectId.Value);

        if (query.WorkspaceId.HasValue)
            dbQuery = dbQuery.Where(x => x.WorkspaceId == query.WorkspaceId.Value);

        if (query.PlatformExtensionId.HasValue)
            dbQuery = dbQuery.Where(x => x.PlatformExtensionId == query.PlatformExtensionId.Value);

        if (query.ContentId.HasValue)
            dbQuery = dbQuery.Where(x => x.ContentId == query.ContentId.Value);

        var resources = await dbQuery
            .OrderBy(x => x.Name)
            .ProjectToType<ResourceItemDto>()
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<ResourceItemDto>>(resources);
    }
}
