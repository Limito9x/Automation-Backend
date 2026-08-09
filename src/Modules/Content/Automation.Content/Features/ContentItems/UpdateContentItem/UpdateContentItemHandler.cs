using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Automation.DynamicForms.Contracts;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Content.Features.ContentItems.UpdateContentItem;

[Transactional(typeof(ContentDbContext))]
public class UpdateContentItemHandler(ContentDbContext db, ISchemaApi schemaApi)
{
    public async Task<Result<ContentItemDto>> HandleAsync(
        UpdateContentItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = await db.ContentItems
            .Include(x => x.ContentType)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            
        if (item is null) return Result.Fail(new NotFoundError("ContentItem not found"));
        
        item.Update(request.Name);
        await db.SaveChangesAsync(cancellationToken);

        var dataResult = await schemaApi.SaveDataAsync(
            "ContentType", 
            item.ContentTypeId.ToString(), 
            item.Id.ToString(), 
            item.ContentType.Key, 
            request.Values, 
            cancellationToken);

        if (dataResult.IsFailed)
        {
            return dataResult.ToResult<ContentItemDto>();
        }
        
        return Result.Ok(new ContentItemDto
        {
            Id = item.Id,
            ContentTypeId = item.ContentTypeId,
            ProjectId = item.ProjectId,
            Name = item.Name,
            Values = dataResult.Value.Values
        });
    }
}
