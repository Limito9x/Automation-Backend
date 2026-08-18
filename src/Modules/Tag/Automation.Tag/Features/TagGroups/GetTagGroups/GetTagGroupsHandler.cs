using Automation.Tag.Infrastructure.Persistence;
using Automation.Tag.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Tag.Features.TagGroups.GetTagGroups;

[NonTransactional]
public class GetTagGroupsHandler(TagDbContext db)
{
    public async Task<Result<IReadOnlyList<TagGroupDto>>> HandleAsync(
        GetTagGroupsQuery query,
        CancellationToken ct
    )
    {
        var groups = db.TagGroups.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Scope))
            groups = groups.Where(x => x.Scope == query.Scope);

        var result = await groups
            .Where(x => x.ProjectId == query.ProjectId)
            .OrderBy(x => x.Scope)
            .ThenBy(x => x.Name)
            .ProjectToType<TagGroupDto>()
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<TagGroupDto>>(result);
    }
}
