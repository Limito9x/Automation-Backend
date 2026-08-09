using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Automation.DynamicForms.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Automation.Content.Features.ContentItems.UpdateContentItem;

public class UpdateContentItemHandler(ContentDbContext db, ISchemaApi schemaApi)
{
    public async Task<Result<ContentItemDto>> HandleAsync(
        UpdateContentItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = await db.ContentItems.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (item is null) return Result.Fail(new NotFoundError("ContentItem not found"));
        
        var dataResult = await schemaApi.SaveDataAsync(
            "ContentType", 
            item.ContentTypeId.ToString(), 
            item.Id.ToString(), 
            "ContentItem", 
            request.Values, 
            cancellationToken);

        if (dataResult.IsFailed)
        {
            return dataResult.ToResult();
        }

        item.Update(request.Name);
        await db.SaveChangesAsync(cancellationToken);
        
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
