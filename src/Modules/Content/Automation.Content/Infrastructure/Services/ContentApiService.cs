using Automation.Content.Contracts;
using Automation.Content.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Automation.Content.Infrastructure.Services;

public class ContentApiService(ContentDbContext db) : IContentApi
{
    public async Task<Result<IReadOnlyDictionary<Guid, ContentSummaryDto>>> GetContentsByProjectIdAsync(
        Guid projectId, 
        CancellationToken ct = default)
    {
        var items = await db.ContentItems
            .AsNoTracking()
            .Where(c => c.ProjectId == projectId)
            .Select(c => new ContentSummaryDto(
                c.Id,
                c.Name,
                c.ContentTypeId,
                c.ContentType.Name,
                c.ContentType.Color,
                c.ContentType.Icon
            ))
            .ToListAsync(ct);

        var map = items.ToDictionary(x => x.Id);
        return Result.Ok<IReadOnlyDictionary<Guid, ContentSummaryDto>>(map);
    }
}
