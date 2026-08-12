using Automation.Resource.Infrastructure.Persistence;
using Automation.Resource.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Resource.Features.Resources.GetResources;

public class GetResourcesHandler(ResourceDbContext db)
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

