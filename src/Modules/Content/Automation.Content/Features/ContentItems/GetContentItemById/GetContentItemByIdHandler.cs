using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Content.Features.ContentItems.GetContentItemById;

public class GetContentItemByIdHandler(ContentDbContext db)
{
    public async Task<Result<ContentItemDto>> HandleAsync(
        GetContentItemByIdQuery query,
        CancellationToken ct)
    {
        var item = await db.ContentItems.FirstOrDefaultAsync(x => x.Id == query.Id, ct);
        if (item is null) return Result.Fail("ContentItem not found");
        
        return Result.Ok(new ContentItemDto(
            item.Id,
            item.ContentTypeId,
            item.ProjectId,
            item.Name,
            item.Values
        ));
    }
}
