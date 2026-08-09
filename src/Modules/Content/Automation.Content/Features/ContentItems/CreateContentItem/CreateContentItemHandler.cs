using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Automation.DynamicForms.Contracts;

namespace Automation.Content.Features.ContentItems.CreateContentItem;

public class CreateContentItemHandler(ContentDbContext db, ISchemaApi schemaApi)
{
    public async Task<Result<ContentItemDto>> HandleAsync(
        CreateContentItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = new ContentItem(
            request.ContentTypeId,
            request.ProjectId,
            request.Name
        );
        
        var dataResult = await schemaApi.SaveDataAsync(
            "ContentType", 
            item.ContentTypeId.ToString(), 
            item.Id.ToString(), 
            "ContentItem", 
            request.Values, 
            cancellationToken);

        if (dataResult.IsFailed)
        {
            return dataResult.ToResult<ContentItemDto>();
        }

        db.ContentItems.Add(item);
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
