using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Content.Features.ContentItems.UpdateContentItem;

public class UpdateContentItemHandler(ContentDbContext db)
{
    public async Task<Result<ContentItemDto>> HandleAsync(
        UpdateContentItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = await db.ContentItems.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (item is null) return Result.Fail("ContentItem not found");
        
        item.Update(request.Name, request.Values);
        await db.SaveChangesAsync(cancellationToken);
        
        return Result.Ok(new ContentItemDto(
            item.Id,
            item.ContentTypeId,
            item.ProjectId,
            item.Name,
            item.Values
        ));
    }
}
