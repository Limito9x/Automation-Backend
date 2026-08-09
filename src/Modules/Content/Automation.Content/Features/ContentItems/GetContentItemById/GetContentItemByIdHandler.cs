using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Automation.DynamicForms.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Automation.Content.Features.ContentItems.GetContentItemById;

public class GetContentItemByIdHandler(ContentDbContext db, ISchemaApi schemaApi)
{
    public async Task<Result<ContentItemDto>> HandleAsync(
        GetContentItemByIdQuery query,
        CancellationToken ct)
    {
        var item = await db.ContentItems.FirstOrDefaultAsync(x => x.Id == query.Id, ct);
        if (item is null) return Result.Fail(new NotFoundError("ContentItem not found"));
        
        var dataResult = await schemaApi.GetDataAsync(item.Id.ToString(), "ContentItem", ct);

        return Result.Ok(new ContentItemDto
        {
            Id = item.Id,
            ContentTypeId = item.ContentTypeId,
            ProjectId = item.ProjectId,
            Name = item.Name,
            Values = dataResult.IsSuccess ? dataResult.Value.Values : null
        });
    }
}
