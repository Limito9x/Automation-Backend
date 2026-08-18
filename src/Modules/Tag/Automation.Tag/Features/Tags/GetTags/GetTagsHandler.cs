using Automation.Tag.Infrastructure.Persistence;
using Automation.Tag.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Tag.Features.Tags.GetTags;

[NonTransactional]
public class GetTagsHandler(TagDbContext db)
{
    public async Task<Result<IReadOnlyList<TagItemDto>>> HandleAsync(GetTagsQuery query, CancellationToken ct)
    {
        var tags = db.TagItems.AsNoTracking();

        if (query.TagGroupId.HasValue)
            tags = tags.Where(x => x.TagGroupId == query.TagGroupId.Value);

        var result = await tags
            .OrderBy(x => x.Name)
            .ProjectToType<TagItemDto>()
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<TagItemDto>>(result);
    }
}