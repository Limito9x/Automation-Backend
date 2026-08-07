using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;

namespace Automation.Content.Features.ContentItems.CreateContentItem;

public class CreateContentItemHandler(ContentDbContext db)
{
    public async Task<Result<ContentItemDto>> HandleAsync(
        CreateContentItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = new ContentItem(
            request.ContentTypeId,
            request.ProjectId,
            request.Name,
            request.Values
        );
        
        db.ContentItems.Add(item);
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
