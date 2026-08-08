using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Content.Features.ContentTypes.GetContentTypeById;

public class GetContentTypeByIdHandler(ContentDbContext db)
{
    public async Task<Result<ContentTypeDto>> HandleAsync(
        GetContentTypeByIdQuery query,
        CancellationToken ct)
    {
        var item = await db.ContentTypes.FirstOrDefaultAsync(x => x.Id == query.Id, ct);
        if (item is null) return Result.Fail(new NotFoundError("ContentType not found"));
        
        return Result.Ok(new ContentTypeDto(
            item.Id,
            item.ProjectId,
            item.Key,
            item.Name,
            item.DisplayName,
            item.Description,
            item.Icon,
            item.Color,
            item.SortOrder,
            item.FieldsConfig,
            item.DisplayConfig
        ));
    }
}
